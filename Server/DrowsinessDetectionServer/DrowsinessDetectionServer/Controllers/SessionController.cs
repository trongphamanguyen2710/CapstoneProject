using DrowsinessDetectionServer.Models.SessionModel;
using DrowsinessDetectionServer.Services.ScopedServices;
using DrowsinessDetectionServer.Services.SingletonServices;
using Microsoft.AspNetCore.Mvc;
using RoleBaseAuthorizationLibrary;

namespace DrowsinessDetectionServer.Controllers;

[Route("api/session")]
[ApiController]
public class SessionController : ControllerBase
{
    private readonly ISessionService sessionService;
    private readonly ILogService logService;
    private readonly ICacheService cacheService;
    private readonly string caller;
    private string message = string.Empty;

    public SessionController(ISessionService sessionService, ILogService logService, ICacheService cacheService)
    {
        this.sessionService = sessionService;
        this.logService = logService;
        this.cacheService = cacheService;
        caller = ((GetType().Namespace?.Split('.') ?? []).LastOrDefault() + "." ?? "Unknown.") + GetType().Name;
    }

    [HttpPost]
    [Route("start")]
    [Authorize(AuthorizationType.Include, Role.Driver)]
    public async Task<ActionResult<SessionGetResponse>> StartSession(SessionStartRequest request)
    {
        try
        {
            SessionGetResponse response = await sessionService.StartSessionAsync(request);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new SessionGetResponse()
            {
                Session = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    [HttpPut]
    [Route("end")]
    [Authorize]
    public async Task<ActionResult<SessionGetResponse>> EndSession(SessionEndRequest request)
    {
        try
        {
            SessionGetResponse response = await sessionService.EndSessionAsync(request);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new SessionGetResponse()
            {
                Session = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    [HttpGet]
    [Route("driver/all")]
    [Authorize]
    public ActionResult<SessionGetListResponse> GetAllSesionByDriver(long driverId)
    {
        try
        {
            SessionGetListResponse response = sessionService.GetAllSessionByDriverAsync(driverId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new SessionGetListResponse()
            {
                Sessions = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }
}
