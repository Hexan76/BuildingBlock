using System.Diagnostics;
using System.Net.Http.Headers;

using Framework.HttpClient.Abstractions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Framework.HttpClient.Http;

public class RequestInterceptorHandler(
    IHttpInterceptorService interceptorService,
    ILogger<RequestInterceptorHandler> logger,
    IOptions<HttpClientServiceOptions> options) : DelegatingHandler
{
    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-Api-Key",
        "Proxy-Authorization"
    };

    public string ClientName { get; init; } = "Default";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = EnsureCorrelationId(request);
        var sw = Stopwatch.StartNew();

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["HttpClient"] = ClientName,
            ["HttpMethod"] = request.Method.Method,
            ["RequestUri"] = request.RequestUri?.ToString(),
            ["CorrelationId"] = correlationId
        });

        try
        {
            interceptorService.OnBeforeSend(request);
            LogRequest(request, correlationId);

            var response = await base.SendAsync(request, cancellationToken);
            sw.Stop();

            interceptorService.OnAfterSend(response);
            LogResponse(request, response, correlationId, sw.Elapsed);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            interceptorService.OnException(ex);
            logger.LogError(
                ex,
                "HTTP {Client} {Method} {Uri} failed after {ElapsedMs}ms Correlation={CorrelationId}",
                ClientName,
                request.Method,
                request.RequestUri,
                sw.ElapsedMilliseconds,
                correlationId);
            throw;
        }
    }

    private string EnsureCorrelationId(HttpRequestMessage request)
    {
        var headerName = options.Value.CorrelationHeaderName;
        if (request.Headers.TryGetValues(headerName, out var existing))
        {
            var current = existing.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(current))
                return current;
        }

        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
        request.Headers.TryAddWithoutValidation(headerName, correlationId);
        return correlationId;
    }

    private void LogRequest(HttpRequestMessage request, string correlationId)
    {
        logger.LogDebug(
            "HTTP request {Client} {Method} {Uri} Correlation={CorrelationId}",
            ClientName,
            request.Method,
            request.RequestUri,
            correlationId);

        if (!logger.IsEnabled(LogLevel.Trace))
            return;

        logger.LogTrace(
            "HTTP request headers {Client}: {Headers}",
            ClientName,
            FormatHeaders(request.Headers));
    }

    private void LogResponse(
        HttpRequestMessage request,
        HttpResponseMessage response,
        string correlationId,
        TimeSpan elapsed)
    {
        var status = (int)response.StatusCode;
        if (status >= 400)
        {
            logger.LogWarning(
                "HTTP {Client} {Method} {Uri} -> {StatusCode} in {ElapsedMs}ms Correlation={CorrelationId}",
                ClientName,
                request.Method,
                request.RequestUri,
                status,
                (int)elapsed.TotalMilliseconds,
                correlationId);
        }
        else
        {
            logger.LogDebug(
                "HTTP {Client} {Method} {Uri} -> {StatusCode} in {ElapsedMs}ms Correlation={CorrelationId}",
                ClientName,
                request.Method,
                request.RequestUri,
                status,
                (int)elapsed.TotalMilliseconds,
                correlationId);
        }

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace(
                "HTTP response headers {Client}: {Headers}",
                ClientName,
                FormatHeaders(response.Headers));
        }
    }

    private static string FormatHeaders(HttpHeaders headers)
        => string.Join("; ", headers.Select(h =>
            SensitiveHeaders.Contains(h.Key)
                ? $"{h.Key}=***"
                : $"{h.Key}={string.Join(",", h.Value)}"));
}
