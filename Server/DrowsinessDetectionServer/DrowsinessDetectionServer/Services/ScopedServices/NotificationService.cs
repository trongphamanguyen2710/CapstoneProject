using DrowsinessDetectionServer.Datas;
using DrowsinessDetectionServer.Models.NotificationModel;
using DrowsinessDetectionServer.Services.SingletonServices;
using DrowsinessDetectionServer.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using DrowsinessDetectionServer.Models.DatabaseModels;
using Newtonsoft.Json;
using DrowsinessDetectionServer.Enum;
using System.Threading.Tasks;

namespace DrowsinessDetectionServer.Services.ScopedServices;

public class NotificationService : INotificationService
{
    private readonly IHubContext<ChatHub> hubContext;
    private readonly ILogService logService;
    private readonly ICacheService cacheService;
    private readonly ApplicationDbContext dbContext;
    private readonly string caller;
    private string message = string.Empty;

    public NotificationService(IHubContext<ChatHub> hubContext, ILogService logService, ICacheService cacheService, ApplicationDbContext dbContext)
    {
        this.hubContext = hubContext;
        this.logService = logService;
        this.cacheService = cacheService;
        this.dbContext = dbContext;
        caller = ((GetType().Namespace?.Split('.') ?? []).LastOrDefault() + "." ?? "Unknown.") + GetType().Name;
    }

    public async Task<NotificationGetResponse> CreateNotificationAsync(NotificationCreateRequest request)
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
                    Notification = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not Found: Detection log not found",
                };
            }
            Supervisor? supervisor = await dbContext.Supervisors.FirstOrDefaultAsync(x => x.Id == request.SupervisorId);
            if (supervisor == null)
            {
                message = $"Supervisor not found (SupervisorId {request.SupervisorId})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    Notification = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not Found: Supervisor not found",
                };
            }
            NotificationLog newNotification = new(detection, supervisor);
            dbContext.NotificationLogs.Add(newNotification);
            await dbContext.SaveChangesAsync();
            string notificationJson = JsonConvert.SerializeObject(newNotification, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            if (ChatHub.ClientConnections.TryGetValue(request.SupervisorId, out string? connectionId)) await hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", notificationJson);
            return new()
            {
                Notification = newNotification,
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
                Notification = null,
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
                Notification = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    public async Task<NotificationGetListResponse> GetAllNotificationBySupervisorAsync(long supervisorId)
    {
        try
        {
            List<NotificationLog> notificationLogs = [.. dbContext.NotificationLogs.Where(x => x.SupervisorId == supervisorId)];
            foreach (NotificationLog notificationLog in notificationLogs) notificationLog.Status = NotificationStatus.Received;
            await dbContext.SaveChangesAsync();
            return new()
            {
                Notifications = notificationLogs,
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
                Notifications = null,
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
                Notifications = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }

    public async Task<NotificationGetResponse> ChangeNotificationStatusAsync(NotificationChangeStatusRequest request)
    {
        try
        {
            NotificationLog? notificationLog = await dbContext.NotificationLogs.FirstOrDefaultAsync(x => x.Id == request.NotificationsId);
            if (notificationLog == null)
            {
                message = $"Notification log not found (NotificationId {request.NotificationsId})";
                logService.Logging(LogLevel.Debug, message, caller);
                return new()
                {
                    Notification = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not Found: Notification log not found",
                };
            }
            notificationLog.Status = request.Status;
            await dbContext.SaveChangesAsync();
            return new()
            {
                Notification = notificationLog,
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
                Notification = null,
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
                Notification = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            };
        }
    }
}

public interface INotificationService
{
    Task<NotificationGetResponse> CreateNotificationAsync(NotificationCreateRequest request);
    Task<NotificationGetListResponse> GetAllNotificationBySupervisorAsync(long supervisorId);
    Task<NotificationGetResponse> ChangeNotificationStatusAsync(NotificationChangeStatusRequest request);
}
