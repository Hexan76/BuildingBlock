using Framework.HttpClient.Abstractions;
using Microsoft.Extensions.Logging;

namespace Framework.HttpClient.Http;

/// <summary>
/// Default request/response interceptor callbacks.
/// Wired into the pipeline via <see cref="RequestInterceptorHandler"/>.
/// </summary>
public class HttpInterceptorService(ILogger<HttpInterceptorService> logger) : IHttpInterceptorService
{
    public virtual void OnBeforeSend(HttpRequestMessage request)
    {
        logger.LogDebug("[Request] {Method} {RequestUri}", request.Method, request.RequestUri);
    }

    public virtual void OnAfterSend(HttpResponseMessage response)
    {
        logger.LogDebug("[Response] {StatusCode}", response.StatusCode);
    }

    public virtual void OnException(Exception ex)
    {
        logger.LogError(ex, "HTTP interceptor captured exception");
    }
}
