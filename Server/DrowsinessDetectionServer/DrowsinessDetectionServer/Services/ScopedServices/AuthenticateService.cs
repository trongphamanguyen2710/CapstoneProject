using DrowsinessDetectionServer.Datas;
using DrowsinessDetectionServer.Models;
using DrowsinessDetectionServer.Models.AuthenticateModels;
using DrowsinessDetectionServer.Models.DatabaseModels;
using DrowsinessDetectionServer.Services.SingletonServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RoleBaseAuthorizationLibrary;

namespace DrowsinessDetectionServer.Services.ScopedServices;

public class AuthenticateService : IAuthenticateService
{
    private readonly IConfiguration configuration;
    private readonly ILogService logService;
    private readonly IJwtService jwtService;
    private readonly ICacheService cacheService;
    private readonly ApplicationDbContext dbContext;
    private readonly string caller;
    private string message = string.Empty;

    public AuthenticateService(ApplicationDbContext dbContext, ILogService logService, IJwtService jwtService, ICacheService cacheService, IConfiguration configuration)
    {
        this.dbContext = dbContext;
        this.configuration = configuration;
        this.logService = logService;
        this.jwtService = jwtService;
        this.cacheService = cacheService;
        caller = ((GetType().Namespace?.Split('.') ?? []).LastOrDefault() + "." ?? "Unknown.") + GetType().Name;
    }

    public Task<BaseResponse> RegisterUserAsync(RegisterRequest request) => RegisterEntityAsync<User>(request);

    public Task<BaseResponse> RegisterDriverAsync(RegisterRequest request) => RegisterEntityAsync<Driver>(request);

    public Task<BaseResponse> RegisterSupervisorAsync(RegisterRequest request) => RegisterEntityAsync<Supervisor>(request);

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName == request.UserName);
            if (user == null || !VerifyPassword(request.Password, user.Password))
            {
                message = $"Wrong user name or password (UserName {request.UserName})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    User = null,
                    Token = string.Empty,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Bad Request: Wrong user name or password",
                };
            }
            AuthorizationUser authorizationUser = new() { Id = user.Id, Role = user.Role };
            string jwtToken = jwtService.GenerateJwtToken(authorizationUser);
            if (string.IsNullOrWhiteSpace(jwtToken)) throw new("Empty token");
            cacheService.AddOrUpdateCacheById(CacheData.USER_CACHE_KEY, user.Id, user);
            message = $"Login success (UserId {user.Id})";
            logService.Logging(LogLevel.Debug, message, caller);
            return new()
            {
                User = user,
                Token = jwtToken,
                StatusCode = StatusCodes.Status200OK,
                Message = "Ok",
            };
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return new()
            {
                User = null,
                Token = string.Empty,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    public async Task<BaseResponse> LogOutAsync(string token)
    {
        try
        {
            await AuthorizationData.RemoveTokenAsync(token);
            message = $"Log out success";
            logService.Logging(LogLevel.Debug, message, caller);
            return new()
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Ok",
            };
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return new()
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    public async Task<BaseResponse> ChangePasswordAsync(ChangePasswordRequest request, User currentUser, string token)
    {
        try
        {
            if (!VerifyPassword(request.OldPassword, currentUser.Password))
            {
                message = $"Wrong password (UserId {currentUser.Id})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized: Wrong password",
                };
            }
            if (request.OldPassword == request.NewPassword)
            {
                message = $"New password is the same as old password (UserId {currentUser.Id})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Bad Request: New password is the same as old password",
                };
            }
            if (request.NewPassword != request.ConfirmPassword)
            {
                message = $"Confirm password mismatch (UserId {currentUser.Id})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Bad Request: Confirm password mismatch",
                };
            }
            await AuthorizationData.RemoveTokenAsync(token);
            currentUser.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            cacheService.AddOrUpdateCacheById(CacheData.USER_CACHE_KEY, currentUser.Id, currentUser);
            await dbContext.SaveChangesAsync();
            message = $"Change password success (UserId {currentUser.Id})";
            logService.Logging(LogLevel.Debug, message, caller);
            return new()
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Ok",
            };
        }
        catch (DbUpdateException ex)
        {
            message = "Database Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            foreach (EntityEntry entry in ex.Entries) await entry.ReloadAsync();
            return new()
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return new()
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    public async Task<ResetPasswordResponse> ResetPasswordAsync(long userId)
    {
        try
        {
            User? user = await cacheService.GetByIdAsync<User>(CacheData.USER_CACHE_KEY, userId, true);
            if (user == null)
            {
                message = $"User not found (UserId {userId})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    ResetPassword = string.Empty,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not Found: User not found",
                };
            }
            _ = AuthorizationData.RemoveTokensAsync(userId);
            string? defaultPassword = configuration.GetSection("DefaultPassword").Get<string>();
            if (string.IsNullOrWhiteSpace(defaultPassword)) defaultPassword = "12345678";
            user.Password = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
            cacheService.AddOrUpdateCacheById(CacheData.USER_CACHE_KEY, user.Id, user);
            await dbContext.SaveChangesAsync();
            message = $"Reset password success (UserId {userId})";
            logService.Logging(LogLevel.Information, message, caller);
            return new()
            {
                ResetPassword = defaultPassword,
                StatusCode = StatusCodes.Status200OK,
                Message = "Ok",
            };
        }
        catch (DbUpdateException ex)
        {
            message = "Database Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            foreach (EntityEntry entry in ex.Entries) await entry.ReloadAsync();
            return new()
            {
                ResetPassword = string.Empty,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return new()
            {
                ResetPassword = string.Empty,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    private async Task<BaseResponse> RegisterEntityAsync<T>(RegisterRequest request) where T : User, new()
    {
        DbSet<T> dbSet = dbContext.Set<T>();
        if (await dbSet.FirstOrDefaultAsync(x => x.UserName == request.UserName) != null)
        {
            string entityName = typeof(T).Name;
            message = $"Register failed, {entityName} name ({request.UserName}) already exists";
            logService.Logging(LogLevel.Debug, message, caller);
            return new()
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = $"Bad Request: {entityName} name already exists",
            };
        }
        T newEntity = new()
        {
            UserName = request.UserName,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
        };
        dbSet.Add(newEntity);
        await dbContext.SaveChangesAsync();
        cacheService.AddOrUpdateCacheById(CacheData.USER_CACHE_KEY, newEntity.Id, newEntity);
        message = $"Register success ({typeof(T).Name}Id {newEntity.Id})";
        logService.Logging(LogLevel.Debug, message, caller);
        return new()
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Ok",
        };
    }

    private static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}

public interface IAuthenticateService
{
    Task<BaseResponse> RegisterUserAsync(RegisterRequest request);
    Task<BaseResponse> RegisterDriverAsync(RegisterRequest request);
    Task<BaseResponse> RegisterSupervisorAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<BaseResponse> LogOutAsync(string token);
    Task<BaseResponse> ChangePasswordAsync(ChangePasswordRequest request, User currentUser, string token);
    Task<ResetPasswordResponse> ResetPasswordAsync(long userId);
}
