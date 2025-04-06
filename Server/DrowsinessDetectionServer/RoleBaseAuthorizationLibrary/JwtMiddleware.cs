using Microsoft.AspNetCore.Http;
using System.Text.Json;

#pragma warning disable IDE0290 // Use primary constructor
namespace RoleBaseAuthorizationLibrary;

public class JwtMiddleware
{
    private readonly RequestDelegate next;

    public JwtMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context, IJwtService jwtService)
    {
        try
        {
            string? token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
            if (token != null)
            {
                long? userId = await jwtService.ValidateJwtToken(token);
                if (userId != null) context.Items["AuthorizationUser"] = await AuthorizationData.GetUserByToken(token);
            }
            await next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            dynamic errorResponse = new
            {
                Status = 500,
                Description = "Internal Server Error: " + ex.Message,
            };
            string json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
    }
}
