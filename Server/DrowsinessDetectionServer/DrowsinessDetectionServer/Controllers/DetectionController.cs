using DrowsinessDetectionServer.Models.DetectionModel;
using DrowsinessDetectionServer.Services.ScopedServices;
using DrowsinessDetectionServer.Services.SingletonServices;
using Microsoft.AspNetCore.Mvc;
using RoleBaseAuthorizationLibrary;

namespace DrowsinessDetectionServer.Controllers;

[Route("api/detection")]
[ApiController]
public class DetectionController : ControllerBase
{
    private readonly IDetectionService detectionService;
    private readonly ILogService logService;
    private readonly ICacheService cacheService;
    private readonly string caller;
    private string message = string.Empty;

    public DetectionController(IDetectionService detectionService, ILogService logService, ICacheService cacheService)
    {
        this.detectionService = detectionService;
        this.logService = logService;
        this.cacheService = cacheService;
        caller = ((GetType().Namespace?.Split('.') ?? []).LastOrDefault() + "." ?? "Unknown.") + GetType().Name;
    }

    [HttpPost]
    [Route("create")]
    [Authorize(AuthorizationType.Include, Role.Driver)]
    public async Task<ActionResult<DetectionGetResponse>> StartSession(DetectionCreateRequest request)
    {
        try
        {
            DetectionGetResponse response = await detectionService.CreateDetectionAsync(request);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new DetectionGetResponse()
            {
                Detection = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    [HttpPut]
    [Route("response")]
    [Authorize]
    public async Task<ActionResult<DetectionGetResponse>> EndSession(DetectionResponseRequest request)
    {
        try
        {
            DetectionGetResponse response = await detectionService.ResponseDetectionAsync(request);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new DetectionGetResponse()
            {
                Detection = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    [HttpGet]
    [Route("driver/all")]
    [Authorize]
    public ActionResult<DetectionGetListResponse> GetAllDetectionByDriver(long driverId)
    {
        try
        {
            DetectionGetListResponse response = detectionService.GetAllDetectionLogByDriverAsync(driverId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new DetectionGetListResponse()
            {
                DetectionLogs = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    [HttpGet]
    [Route("session/all")]
    [Authorize]
    public ActionResult<DetectionGetListResponse> GetAllDetectionBySession(long sessionId)
    {
        try
        {
            DetectionGetListResponse response = detectionService.GetAllDetectionLogBySessionAsync(sessionId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new DetectionGetListResponse()
            {
                DetectionLogs = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }
}
