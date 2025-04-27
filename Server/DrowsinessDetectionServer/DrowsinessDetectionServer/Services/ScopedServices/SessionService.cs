using DrowsinessDetectionServer.Datas;
using DrowsinessDetectionServer.Models.DatabaseModels;
using DrowsinessDetectionServer.Models.SessionModel;
using DrowsinessDetectionServer.Services.SingletonServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DrowsinessDetectionServer.Services.ScopedServices;

public class SessionService : ISessionService
{
    private readonly ILogService logService;
    private readonly ICacheService cacheService;
    private readonly ApplicationDbContext dbContext;
    private readonly string caller;
    private string message = string.Empty;

    public SessionService(ILogService logService, ICacheService cacheService, ApplicationDbContext dbContext)
    {
        this.logService = logService;
        this.cacheService = cacheService;
        this.dbContext = dbContext;
        caller = ((GetType().Namespace?.Split('.') ?? []).LastOrDefault() + "." ?? "Unknown.") + GetType().Name;
    }

    public async Task<SessionGetResponse> StartSessionAsync(SessionStartRequest request)
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
                    Session = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not Found: User not found",
                };
            }
            MonitorSession newSession = new(driver);
            dbContext.MonitorSessions.Add(newSession);
            await dbContext.SaveChangesAsync();
            return new()
            {
                Session = newSession,
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
                Session = null,
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
                Session = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    public async Task<SessionGetResponse> EndSessionAsync(SessionEndRequest request)
    {
        try
        {
            MonitorSession? session = await dbContext.MonitorSessions.FirstOrDefaultAsync(x => x.Id == request.SessionId);
            if (session == null)
            {
                message = $"Session not found (SessionId {request.SessionId})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    Session = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not Found: Session not found",
                };
            }
            session.End();
            await dbContext.SaveChangesAsync();
            return new()
            {
                Session = session,
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
                Session = null,
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
                Session = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    public SessionGetListResponse GetAllSessionByDriverAsync(long driverId)
    {
        try
        {
            List<MonitorSession> sessions = [.. dbContext.MonitorSessions.Where(x => x.DriverId == driverId)];
            return new()
            {
                Sessions = sessions,
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
                Sessions = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }
}

public interface ISessionService
{
    Task<SessionGetResponse> StartSessionAsync(SessionStartRequest request);
    Task<SessionGetResponse> EndSessionAsync(SessionEndRequest request);
    SessionGetListResponse GetAllSessionByDriverAsync(long driverId);
}
