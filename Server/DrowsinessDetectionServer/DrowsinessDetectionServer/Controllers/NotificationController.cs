using DrowsinessDetectionServer.Models.DetectionModel;
using DrowsinessDetectionServer.Models.NotificationModel;
using DrowsinessDetectionServer.Services.ScopedServices;
using DrowsinessDetectionServer.Services.SingletonServices;
using Microsoft.AspNetCore.Mvc;
using RoleBaseAuthorizationLibrary;

namespace DrowsinessDetectionServer.Controllers;

[Route("api/notification")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly INotificationService notificationService;
    private readonly ILogService logService;
    private readonly ICacheService cacheService;
    private readonly string caller;
    private string message = string.Empty;

    public NotificationController(INotificationService notificationService, ILogService logService, ICacheService cacheService)
    {
        this.notificationService = notificationService;
        this.logService = logService;
        this.cacheService = cacheService;
        caller = ((GetType().Namespace?.Split('.') ?? []).LastOrDefault() + "." ?? "Unknown.") + GetType().Name;
    }

    [HttpPost]
    [Route("create")]
    [Authorize(AuthorizationType.Include, Role.Driver)]
    public async Task<ActionResult<NotificationGetResponse>> StartSession(NotificationCreateRequest request)
    {
        try
        {
            NotificationGetResponse response = await notificationService.CreateNotificationAsync(request);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new NotificationGetResponse()
            {
                Notification = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    [HttpPut]
    [Route("status/change")]
    [Authorize]
    public async Task<ActionResult<NotificationGetResponse>> ChangeNotificationStatus(NotificationChangeStatusRequest request)
    {
        try
        {
            NotificationGetResponse response = await notificationService.ChangeNotificationStatusAsync(request);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new NotificationGetResponse()
            {
                Notification = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    [HttpGet]
    [Route("supervisor/all")]
    [Authorize]
    public async Task<ActionResult<NotificationGetListResponse>> GetAllNotificationBySupervisor(long supervisorId)
    {
        try
        {
            NotificationGetListResponse response = await notificationService.GetAllNotificationBySupervisorAsync(supervisorId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new NotificationGetListResponse()
            {
                Notifications = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }
}
