using Framework.BuildingBlock.Application.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Framework.BuildingBlock.HttpApi;

public class FrameworkCookieAuthenticationEvents : CookieAuthenticationEvents
{
    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        var response = new RejectMessage
        {
            Message = "Unauthorized: Authentication required. \r\n You must be logged in to access this resource.",
            Type = MessageType.Error,
        };

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        var response = new RejectMessage
        {
            Message = "Forbidden: Access is denied.\r\n You do not have permission to access this resource.",
            Type = MessageType.Error,
        };

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
