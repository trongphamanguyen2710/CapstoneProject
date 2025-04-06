using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RoleBaseAuthorizationLibrary;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly List<Role> validRole = [];

    public AuthorizeAttribute(AuthorizationType type = AuthorizationType.Include, params Role[] roleList)
    {
        if (type == AuthorizationType.Include) validRole = roleList.ToList() ?? [];
        else
        {
            Role[] roleEnum = Enum.GetValues(typeof(Role)).Cast<Role>().ToArray();
            foreach (Role role in roleEnum) if (!roleList.Contains(role)) validRole.Add(role);
        }
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // skip web socket request
        if (context.HttpContext.WebSockets.IsWebSocketRequest) return;

        // skip authorization if action is decorated with [AllowAnonymous] attribute
        bool allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous) return;

        try
        {
            // authorization
            if (context.HttpContext.Items["AuthorizationUser"] is not AuthorizationUser user)
            {
                // not logged in
                dynamic result = new
                {
                    Status = 401,
                    Description = "Unauthorized",
                };
                context.Result = new JsonResult(result) { StatusCode = StatusCodes.Status401Unauthorized };
            }
            else if (user.Role == Role.Admin)
            {
                // bypass authorization
            }
            else if (validRole.Count != 0 && !validRole.Contains(user.Role))
            {
                // role not authorized
                dynamic result = new
                {
                    Status = 403,
                    Description = "Forbidden",
                };
                context.Result = new JsonResult(result) { StatusCode = StatusCodes.Status403Forbidden };
            }
        }
        catch (Exception ex)
        {
            dynamic result = new
            {
                Status = 500,
                Description = "Unexpected error: " + ex.Message,
            };
            context.Result = new JsonResult(result) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class AllowAnonymousAttribute : Attribute
{

}
