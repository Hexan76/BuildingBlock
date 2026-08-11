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
#if DEBUG
        logger.LogDebug("[Request] {Method} {RequestUri}", request.Method, request.RequestUri);
#endif
    }

    public virtual void OnAfterSend(HttpResponseMessage response)
    {
#if DEBUG
        logger.LogDebug("[Response] {StatusCode}", response.StatusCode);
#endif
    }

    public virtual void OnException(Exception ex)
    {
        logger.LogError(ex, "HTTP interceptor captured exception");
    }
}
