using System.Text.Json;
using Framework.BuildingBlock.Application.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Framework.BuildingBlock.HttpApi;

public static class BuildingBlockStatusCodePagesExtensions
{
    public static IApplicationBuilder UseBuildingBlockStatusCodePages(this IApplicationBuilder app)
    {
        return app.UseStatusCodePages(async context =>
        {
            var http = context.HttpContext;
            var status = http.Response.StatusCode;
            if (http.Response.HasStarted)
            {
                return;
            }

            var message = status switch
            {
                StatusCodes.Status401Unauthorized => "Authentication is required.",
                StatusCodes.Status403Forbidden => "You do not have permission to access this resource.",
                _ => null
            };

            if (message is null)
            {
                return;
            }

            http.Response.ContentType = "application/json";
            var body = MessageContract.ToError(
                status.ToString(),
                [message],
                status,
                $"TraceId :{http.TraceIdentifier}");
            await http.Response.WriteAsync(JsonSerializer.Serialize(body));
        });
    }
}
