using DrowsinessDetectionServer.Models.FaceDataModel;
using DrowsinessDetectionServer.Models.UserModels;
using DrowsinessDetectionServer.Services.ScopedServices;
using DrowsinessDetectionServer.Services.SingletonServices;
using Microsoft.AspNetCore.Mvc;
using RoleBaseAuthorizationLibrary;

namespace DrowsinessDetectionServer.Controllers;

[Route("drownsiness/api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService userService;
    private readonly ILogService logService;
    private readonly ICacheService cacheService;
    private readonly string caller;
    private string message = string.Empty;

    public UserController(IUserService userService, ILogService logService, ICacheService cacheService)
    {
        this.userService = userService;
        this.logService = logService;
        this.cacheService = cacheService;
        caller = ((GetType().Namespace?.Split('.') ?? []).LastOrDefault() + "." ?? "Unknown.") + GetType().Name;
    }

    /// <summary>
    /// Change user information by id
    /// </summary>
    /// <param name="request"></param>
    [HttpPut]
    [Route("update")]
    [Authorize]
    public async Task<ActionResult<UserGetResponse>> ChangeUserInfor(ChangeUserInfoRequest request)
    {
        try
        {
            UserGetResponse response = await userService.ChangeUserInforAsync(request);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new UserGetResponse()
            {
                User = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    /// <summary>
    /// Assign supervisor to driver by id
    /// </summary>
    /// <param name="request"></param>
    [HttpPut]
    [Route("assign")]
    [Authorize]
    public async Task<ActionResult<DriverSupervisorResponse>> AssignSupervisor(AssignSupervisorRequest request)
    {
        try
        {
            DriverSupervisorResponse response = await userService.AssignSupervisorAsync(request);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new DriverSupervisorResponse()
            {
                Driver = null,
                Supervisor = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    /// <summary>
    /// Add face data to driver
    /// </summary>
    /// <param name="request"></param>
    [HttpPost]
    [Route("face/add")]
    [Authorize]
    public async Task<ActionResult<FaceDataGetResponse>> AddDriverFaceData(FaceDataAddRequest request)
    {
        try
        {
            FaceDataGetResponse response = await userService.AddDriverFaceDataAsync(request);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new FaceDataGetResponse()
            {
                FaceData = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    /// <summary>
    /// Edit face data of driver by
    /// </summary>
    /// <param name="request"></param>
    [HttpPost]
    [Route("face/edit")]
    [Authorize]
    public async Task<ActionResult<FaceDataGetResponse>> EditDriverFaceData(FaceDataEditRequest request)
    {
        try
        {
            FaceDataGetResponse response = await userService.EditDriverFaceDataAsync(request);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new FaceDataGetResponse()
            {
                FaceData = null,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }
}
