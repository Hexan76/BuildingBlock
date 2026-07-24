using Framework.BuildingBlock.Data;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.BuildingBlock.Domain;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userIdClaim =
            context.User.FindFirst("sub")
            ?? context.User.FindFirst("userId");

        if (userIdClaim != null &&
            Guid.TryParse(userIdClaim.Value, out var userId))
        {
            CurrentUserContext.UserId = userId;
        }

        await _next(context);
    }
}
public static class CurrentUserExtensions
{
    public static IServiceCollection AddCurrentUser(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();


        services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();


        return services;
    }

    public static IApplicationBuilder UseCurrentUser(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<CurrentUserMiddleware>();
    }
}
