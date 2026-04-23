using Framework.BuildingBlock.Application.Contracts;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Volo.Abp.AspNetCore.Middleware;
using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock.HttpApi;

public class WrapAbpRoutesResponseMiddleware : AbpMiddlewareBase, ISingletonDependency
{
    public async override Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = context.GetEndpoint()?.DisplayName;
        if (path != null && (path.Contains("Abp") && context.Request.Path.Value.Contains("api")))
        {

            // Capture the original response body stream
            var originalResponseBodyStream = context.Response.Body;

            // Create a new memory stream to capture the response
            using (var newResponseBodyStream = new System.IO.MemoryStream())
            {
                context.Response.Body = newResponseBodyStream;

                // Proceed to the next middleware in the pipeline
                await next(context);

                // Reset the position of the memory stream to read the content
                newResponseBodyStream.Seek(0, System.IO.SeekOrigin.Begin);

                // Read the content from the memory stream
                var responseBody = new System.IO.StreamReader(newResponseBodyStream).ReadToEnd();

                // Only wrap success responses (200 OK)
                if (context.Response.StatusCode == StatusCodes.Status200OK)
                {
                    var successResponse = new AcceptMessage<object>
                    {

                        Message = "Success", // You can modify this message as needed
                        Type = MessageType.Success, // Set as "Info" for success
                        Data = JsonSerializer.Deserialize<object>(responseBody) // Wrap actual data
                    };

                    // Reset the response body and write the new response
                    context.Response.Body = originalResponseBodyStream;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(successResponse));
                    return;
                }
                else
                {
                    // If not a 200 OK, just return the original response (no modification)
                    context.Response.Body = originalResponseBodyStream;
                    await context.Response.WriteAsync(responseBody);
                    return;
                }
            }
        }

        await next(context);
    }
}

