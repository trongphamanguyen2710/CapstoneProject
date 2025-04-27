using DrowsinessDetectionServer.Datas;
using DrowsinessDetectionServer.Models.DatabaseModels;
using DrowsinessDetectionServer.Models.FaceDataModel;
using DrowsinessDetectionServer.Models.UserModels;
using DrowsinessDetectionServer.Services.SingletonServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DrowsinessDetectionServer.Services.ScopedServices;

public class UserService : IUserService
{
    private readonly ILogService logService;
    private readonly ICacheService cacheService;
    private readonly ApplicationDbContext dbContext;
    private readonly string caller;
    private string message = string.Empty;

    public UserService(ILogService logService, ICacheService cacheService, ApplicationDbContext dbContext)
    {
        this.logService = logService;
        this.cacheService = cacheService;
        this.dbContext = dbContext;
        caller = ((GetType().Namespace?.Split('.') ?? []).LastOrDefault() + "." ?? "Unknown.") + GetType().Name;
    }

    public async Task<UserGetResponse> ChangeUserInforAsync(ChangeUserInfoRequest request)
    {
        try
        {
            User? user = await cacheService.GetByIdAsync<User>(CacheData.USER_CACHE_KEY, request.UserId, true);
            if (user == null)
            {
                message = $"User not found (UserId {request.UserId})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    User = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not Found: User not found",
                };
            }
            user.Update(request);
            cacheService.AddOrUpdateCacheById(CacheData.USER_CACHE_KEY, request.UserId, user);
            await dbContext.SaveChangesAsync();
            message = $"Change user infor success (UserId {request.UserId})";
            logService.Logging(LogLevel.Information, message, caller);
            return new()
            {
                User = user,
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
                User = null,
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
                User = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    public async Task<DriverSupervisorResponse> AssignSupervisorAsync(AssignSupervisorRequest request)
    {
        try
        {
            Driver? driver = await cacheService.GetByIdAsync<Driver>(CacheData.USER_CACHE_KEY, request.DriverId, true);
            if (driver == null)
            {
                message = $"Driver not found (UserId {request.DriverId})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    Driver = null,
                    Supervisor = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not Found: User not found",
                };
            }
            Supervisor? supervisor = await cacheService.GetByIdAsync<Supervisor>(CacheData.USER_CACHE_KEY, request.SupervisorId, true);
            if (supervisor == null)
            {
                message = $"Supervisor not found (UserId {request.SupervisorId})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    Driver = null,
                    Supervisor = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not Found: User not found",
                };
            }
            driver.Supervisor = supervisor;
            cacheService.AddOrUpdateCacheById(CacheData.USER_CACHE_KEY, request.DriverId, driver);
            cacheService.AddOrUpdateCacheById(CacheData.USER_CACHE_KEY, request.SupervisorId, supervisor);
            await dbContext.SaveChangesAsync();
            return new()
            {
                Driver = driver,
                Supervisor = supervisor,
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
                Driver = null,
                Supervisor = null,
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
                Driver = null,
                Supervisor = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    public async Task<FaceDataGetResponse> AddDriverFaceDataAsync(FaceDataAddRequest request)
    {
        try
        {
            Driver? driver = await dbContext.Drivers.Include(x => x.FaceData).FirstOrDefaultAsync(x => x.Id == request.DriverId);
            if (driver == null)
            {
                message = $"Driver not found (UserId {request.DriverId})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    FaceData = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not Found: User not found",
                };
            }
            if (driver.FaceData != null)
            {
                message = $"Driver already has face data";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    FaceData = null,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Bad Request: Driver already has face data",
                };
            }
            FaceData newFaceData = new(123, 456, "Sample 1", "Sample 2", driver);
            dbContext.FaceDatas.Add(newFaceData);
            await dbContext.SaveChangesAsync();
            cacheService.AddOrUpdateCacheById(CacheData.FACE_CACHE_KEY, newFaceData.Id, newFaceData);
            return new()
            {
                FaceData = newFaceData,
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
                FaceData = null,
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
                FaceData = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    public async Task<FaceDataGetResponse> EditDriverFaceDataAsync(FaceDataEditRequest request)
    {
        try
        {
            FaceData? faceData = await cacheService.GetByIdAsync<FaceData>(CacheData.FACE_CACHE_KEY, request.FaceDataId, true);
            if (faceData == null)
            {
                message = $"Face data not found (FaceDataId {request.FaceDataId})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    FaceData = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not Found: Face data not found",
                };
            }
            faceData.Update(request);
            cacheService.AddOrUpdateCacheById(CacheData.FACE_CACHE_KEY, faceData.Id, faceData);
            await dbContext.SaveChangesAsync();
            return new()
            {
                FaceData = faceData,
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
                FaceData = null,
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
                FaceData = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }
}

public interface IUserService
{
    Task<UserGetResponse> ChangeUserInforAsync(ChangeUserInfoRequest request);
    Task<DriverSupervisorResponse> AssignSupervisorAsync(AssignSupervisorRequest request);
    Task<FaceDataGetResponse> AddDriverFaceDataAsync(FaceDataAddRequest request);
    Task<FaceDataGetResponse> EditDriverFaceDataAsync(FaceDataEditRequest request);
}
