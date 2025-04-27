using DrowsinessDetectionServer.Datas;
using DrowsinessDetectionServer.Models.DatabaseModels;
using DrowsinessDetectionServer.Models.DetectionModel;
using DrowsinessDetectionServer.Services.SingletonServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DrowsinessDetectionServer.Services.ScopedServices;

public class DetectionService : IDetectionService
{
    private readonly ILogService logService;
    private readonly ICacheService cacheService;
    private readonly ApplicationDbContext dbContext;
    private readonly string caller;
    private string message = string.Empty;

    public DetectionService(ILogService logService, ICacheService cacheService, ApplicationDbContext dbContext)
    {
        this.logService = logService;
        this.cacheService = cacheService;
        this.dbContext = dbContext;
        caller = ((GetType().Namespace?.Split('.') ?? []).LastOrDefault() + "." ?? "Unknown.") + GetType().Name;
    }

    public async Task<DetectionGetResponse> CreateDetectionAsync(DetectionCreateRequest request)
    {
        try
        {
            MonitorSession? session = await dbContext.MonitorSessions.Include(x => x.Driver).FirstOrDefaultAsync(x => x.Id == request.SessionId);
            if (session == null)
            {
                message = $"Session not found (SessionId {request.SessionId})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    Detection = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not Found: User not found",
                };
            }
            DetectionLog newDetection = new(request, session, session.Driver);
            dbContext.DetectionLogs.Add(newDetection);
            await dbContext.SaveChangesAsync();
            return new()
            {
                Detection = newDetection,
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
                Detection = null,
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
                Detection = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    public async Task<DetectionGetResponse> ResponseDetectionAsync(DetectionResponseRequest request)
    {
        try
        {
            DetectionLog? detection = await dbContext.DetectionLogs.FirstOrDefaultAsync(x => x.Id == request.DetectionId);
            if (detection == null)
            {
                message = $"Detection log not found (DetectionId {request.DetectionId})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    Detection = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not Found: Detection log not found",
                };
            }
            detection.Response();
            await dbContext.SaveChangesAsync();
            return new()
            {
                Detection = detection,
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
                Detection = null,
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
                Detection = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    public DetectionGetListResponse GetAllDetectionLogByDriverAsync(long driverId)
    {
        try
        {
            List<DetectionLog> detection = [.. dbContext.DetectionLogs.Where(x => x.DriverId == driverId)];
            return new()
            {
                DetectionLogs = detection,
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
                DetectionLogs = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    public DetectionGetListResponse GetAllDetectionLogBySessionAsync(long sessionId)
    {
        try
        {
            List<DetectionLog> detection = [.. dbContext.DetectionLogs.Where(x => x.SessionId == sessionId)];
            return new()
            {
                DetectionLogs = detection,
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
                DetectionLogs = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }
}

public interface IDetectionService
{
    Task<DetectionGetResponse> CreateDetectionAsync(DetectionCreateRequest request);
    Task<DetectionGetResponse> ResponseDetectionAsync(DetectionResponseRequest request);
    DetectionGetListResponse GetAllDetectionLogByDriverAsync(long driverId);
    DetectionGetListResponse GetAllDetectionLogBySessionAsync(long sessionId);
}
