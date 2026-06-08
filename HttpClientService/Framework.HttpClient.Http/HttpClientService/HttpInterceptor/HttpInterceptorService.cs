using Framework.HttpClient.Abstractions;
using Microsoft.Extensions.Logging;

namespace Framework.HttpClient.Http;

public class HttpInterceptorService : DelegatingHandler, IHttpInterceptorService
{
    private readonly ILogger<HttpInterceptorService> _logger;

    public HttpInterceptorService(ILogger<HttpInterceptorService> logger)
    {
        _logger = logger;
    }

    public virtual void OnBeforeSend(HttpRequestMessage request)
    {
#if DEBUG
        _logger.LogDebug("[Request] {Method} {RequestUri}", request.Method, request.RequestUri);
#endif
    }

    public virtual void OnAfterSend(HttpResponseMessage response)
    {
#if DEBUG
        _logger.LogDebug("[Response] {StatusCode}", response.StatusCode);
#endif
    }

    public virtual void OnException(Exception ex)
    {
        _logger.LogError(ex, "HTTP interceptor captured exception");
    }
}
