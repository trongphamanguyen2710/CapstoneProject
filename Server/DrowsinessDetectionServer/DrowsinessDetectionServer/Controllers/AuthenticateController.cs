using DrowsinessDetectionServer.Models;
using DrowsinessDetectionServer.Models.AuthenticateModels;
using DrowsinessDetectionServer.Models.DatabaseModels;
using DrowsinessDetectionServer.Services.ScopedServices;
using DrowsinessDetectionServer.Services.SingletonServices;
using Microsoft.AspNetCore.Mvc;
using RoleBaseAuthorizationLibrary;
using System.ComponentModel.DataAnnotations;

namespace DrowsinessDetectionServer.Controllers;

[Route("drownsiness/api/authen")]
[ApiController]
public class AuthenticateController : ControllerBase
{
    private readonly IAuthenticateService authenticateService;
    private readonly ILogService logService;
    private readonly ICacheService cacheService;
    private readonly string caller;
    private string message = string.Empty;

    public AuthenticateController(IAuthenticateService authenticateService, ILogService logService, ICacheService cacheService)
    {
        this.authenticateService = authenticateService;
        this.logService = logService;
        this.cacheService = cacheService;
        caller = ((GetType().Namespace?.Split('.') ?? []).LastOrDefault() + "." ?? "Unknown.") + GetType().Name;
    }

    /// <summary>
    /// Register a new account
    /// </summary>
    /// <param name="request"></param>
    [HttpPost]
    [Route("register")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse>> Register(RegisterRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                message = "Empty user name";
                logService.Logging(LogLevel.Debug, message, caller);
                return StatusCode(StatusCodes.Status400BadRequest, new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Bad Request: Empty user name",
                });
            }
            if (request.Password.Length < 8)
            {
                message = "Invalid password";
                logService.Logging(LogLevel.Debug, message, caller);
                return StatusCode(StatusCodes.Status400BadRequest, new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Bad Request: Invalid password",
                });
            }
            BaseResponse response = request.Role switch
            {
                Role.Driver => await authenticateService.RegisterDriverAsync(request),
                Role.Supervisor => await authenticateService.RegisterSupervisorAsync(request),
                _ => await authenticateService.RegisterUserAsync(request),
            };
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse()
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    /// <summary>
    /// Login
    /// </summary>
    /// <param name="request"></param>
    /// <remarks>
    /// Return the user information and a token
    /// </remarks>
    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || request.Password.Length < 8)
            {
                message = "Invalid user name or password";
                logService.Logging(LogLevel.Debug, message, caller);
                return StatusCode(StatusCodes.Status400BadRequest, new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Bad Request: Invalid user name or password",
                });
            }
            LoginResponse response = await authenticateService.LoginAsync(request);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponse()
            {
                User = null,
                Token = string.Empty,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    /// <summary>
    /// Log out
    /// </summary>
    [HttpGet]
    [Route("logout")]
    [Authorize]
    public async Task<ActionResult<BaseResponse>> LogOut()
    {
        try
        {
            string? authorizationHeader = Request.Headers.Authorization;
            string token = string.Empty;
            if (!string.IsNullOrWhiteSpace(authorizationHeader) && authorizationHeader.StartsWith("Bearer ")) token = authorizationHeader["Bearer ".Length..].Trim();
            if (string.IsNullOrWhiteSpace(token)) throw new("JWT Bearer token not found in header");
            BaseResponse response = await authenticateService.LogOutAsync(token);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse()
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    /// <summary>
    /// Change password of the current user
    /// </summary>
    /// <param name="request"></param>
    [HttpPut]
    [Route("password/change")]
    [Authorize]
    public async Task<ActionResult<BaseResponse>> ChangePassword(ChangePasswordRequest request)
    {
        try
        {
            User? user = await cacheService.GetCurrentUser(HttpContext, true) ?? throw new("User not found");
            string? authorizationHeader = Request.Headers.Authorization;
            string token = string.Empty;
            if (!string.IsNullOrWhiteSpace(authorizationHeader) && authorizationHeader.StartsWith("Bearer ")) token = authorizationHeader["Bearer ".Length..].Trim();
            if (string.IsNullOrWhiteSpace(token)) throw new("JWT Bearer token not found in header");
            bool validOldPassword = request.OldPassword.Length >= 8;
            bool validNewPassword = request.NewPassword.Length >= 8;
            bool validConfirmPassword = request.ConfirmPassword.Length >= 8;
            if (!validOldPassword || !validNewPassword || !validConfirmPassword)
            {
                message = "Invalid password(s)";
                logService.Logging(LogLevel.Debug, message, caller);
                return StatusCode(StatusCodes.Status400BadRequest, new BaseResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Bad Request: Invalid password(s)",
                });
            }
            BaseResponse response = await authenticateService.ChangePasswordAsync(request, user, token);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse()
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }

    /// <summary>
    /// Reset password of a user
    /// </summary>
    /// <param name="userId"></param>
    /// <remarks>
    /// Can only be used by staff members
    /// </remarks>
    [HttpPut]
    [Route("password/reset")]
    [Authorize(AuthorizationType.Exclude, Role.Admin, Role.Moderator)]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPassword([Required] long userId)
    {
        try
        {
            ResetPasswordResponse response = await authenticateService.ResetPasswordAsync(userId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            message = "Internal Server Error: " + ex.Message;
            logService.Logging(LogLevel.Error, message, caller);
            return StatusCode(StatusCodes.Status500InternalServerError, new ResetPasswordResponse()
            {
                ResetPassword = string.Empty,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = message,
            });
        }
    }
}
