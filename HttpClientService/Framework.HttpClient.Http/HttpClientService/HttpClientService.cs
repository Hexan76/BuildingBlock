using System.Diagnostics;
using System.Text.Json;

using Framework.HttpClient.Abstractions;
using Framework.HttpClient.Abstractions.Exceptions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Framework.HttpClient.Http;

public class HttpClientService(
    IHttpClientFactory httpClientFactory,
    ILogger<HttpClientService> logger,
    IRequestBuilder requestBuilder,
    IFormContentBuilder formContentBuilder,
    IResponseHandlerFactory responseHandlerFactory,
    IOptions<HttpClientServiceOptions> httpOptions,
    IRequestResolver requestResolver,
    IHttpClientErrorPolicyRegistry errorPolicyRegistry) : IHttpClientService
{
    public Task<TResponse> SendAsync<TResponse>(
        IHttpRequest request,
        ResponseType WrapType = ResponseType.Default,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null,
        string contentType = "application/json")
        where TResponse : class
        => SendAsync<TResponse>(request, CancellationToken.None, WrapType, clientName, customHeaders, contentType);

    public async Task<TResponse> SendAsync<TResponse>(IHttpRequest request, CancellationToken cancellationToken, ResponseType WrapType = ResponseType.Default, string clientName = "", Dictionary<string, string>? customHeaders = null, string contentType = "application/json")
        where TResponse : class
    {
        var resolvedClientName = ResolveClientName(clientName);
        var client = httpClientFactory.CreateClient(resolvedClientName);
        var message = requestBuilder.Build(request, contentType);
        AddCustomHeaders(message, customHeaders);

        return await SendCoreAsync<TResponse>(
            client,
            resolvedClientName,
            message,
            WrapType,
            cancellationToken);
    }

    public Task<TResponse> SendFormAsync<TResponse>(
        IHttpRequest request,
        ResponseType WrapType = ResponseType.Default,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null)
        where TResponse : class
        => SendFormAsync<TResponse>(request, CancellationToken.None, WrapType, clientName, customHeaders);

    public async Task<TResponse> SendFormAsync<TResponse>(
        IHttpRequest request,
        CancellationToken cancellationToken,
        ResponseType WrapType = ResponseType.Default,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null)
        where TResponse : class
    {
        var resolvedClientName = ResolveClientName(clientName);
        var client = httpClientFactory.CreateClient(resolvedClientName);
        var resolved = requestResolver.ResolveRequestFields(request);
        var content = formContentBuilder.Build(resolved.BodyContent ?? request, "multipart/form-data");
        var message = new HttpRequestMessage(request.Method, BuildRelativeUri(resolved.FinalRoute, resolved.QueryParams))
        {
            Content = content
        };
        AddCustomHeaders(message, customHeaders);

        return await SendCoreAsync<TResponse>(
            client,
            resolvedClientName,
            message,
            WrapType,
            cancellationToken);
    }

    public void AddCustomHeaders(HttpRequestMessage message, Dictionary<string, string>? customHeaders)
    {
        if (customHeaders is null)
            return;

        foreach (var kvp in customHeaders)
            message.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
    }

    private async Task<TResponse> SendCoreAsync<TResponse>(
        System.Net.Http.HttpClient client,
        string clientName,
        HttpRequestMessage message,
        ResponseType wrapType,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        var sw = Stopwatch.StartNew();
        var correlationId = GetRequestCorrelationId(message);

        try
        {
            var response = await client.SendAsync(message, cancellationToken);
            await response.Content.LoadIntoBufferAsync();
            sw.Stop();

            if (!response.IsSuccessStatusCode)
            {
                var fault = await CreateFaultAsync(clientName, message, response, sw.Elapsed, cancellationToken);
                LogFault(fault);
                throw new HttpClientFaultException(fault);
            }

            await LogTraceBodyAsync(clientName, response, cancellationToken);

            var handler = responseHandlerFactory.GetHandlerFor(wrapType, typeof(TResponse));
            try
            {
                var result = await handler.HandleAsync(response, typeof(TResponse));
                return (TResponse)result;
            }
            catch (JsonException ex)
            {
                throw new TransportSerializationException(
                    $"Failed to deserialize response from '{clientName}'.",
                    clientName,
                    correlationId,
                    ex);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            sw.Stop();
            logger.LogError(
                ex,
                "HTTP call to {ClientName} timed out after {ElapsedMs}ms Correlation={CorrelationId}",
                clientName,
                sw.ElapsedMilliseconds,
                correlationId);
            throw new TransportTimeoutException(
                $"HTTP call to '{clientName}' timed out.",
                clientName,
                correlationId,
                ex);
        }
        catch (HttpClientFaultException)
        {
            throw;
        }
        catch (TransportException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            logger.LogError(
                ex,
                "HTTP transport failed for {ClientName} {Method} {Uri} after {ElapsedMs}ms Correlation={CorrelationId}",
                clientName,
                message.Method,
                message.RequestUri,
                sw.ElapsedMilliseconds,
                correlationId);
            throw new TransportException(
                $"HTTP call to '{clientName}' failed.",
                clientName,
                correlationId,
                ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HttpClientService failed to send request to {ClientName}", clientName);
            throw;
        }
    }

    private async Task<HttpClientFault> CreateFaultAsync(
        string clientName,
        HttpRequestMessage message,
        HttpResponseMessage response,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        var body = await ReadTruncatedBodyAsync(response, httpOptions.Value.MaxErrorBodyLength, cancellationToken);
        var upstream = TryParseEnvelope(body);
        var statusCode = (int)response.StatusCode;

        return new HttpClientFault
        {
            ClientName = clientName,
            Method = message.Method,
            RequestUri = message.RequestUri?.ToString() ?? string.Empty,
            StatusCode = statusCode,
            PublicStatusCode = errorPolicyRegistry.Get(clientName).ResolvePublicStatus(statusCode),
            ResponseBody = body,
            CorrelationId = GetCorrelationId(message, response),
            Duration = duration,
            UpstreamResult = upstream
        };
    }

    private HttpResultModel? TryParseEnvelope(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        try
        {
            var model = JsonSerializer.Deserialize<HttpResultModel>(body, httpOptions.Value.JsonSerializerOptions);
            return LooksLikeEnvelope(model) ? model : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool LooksLikeEnvelope(HttpResultModel? model)
        => model is not null && (
            model.Error is not null
            || model.Errors is { Length: > 0 }
            || model.Messages is { Length: > 0 }
            || model.Validations is not null
            || (!string.IsNullOrWhiteSpace(model.ApplicationCode) && model.ApplicationCode != "500"));

    private void LogFault(HttpClientFault fault)
    {
        var logLevel = fault.StatusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
        logger.Log(
            logLevel,
            "HTTP call failed. Client={ClientName} {Method} {Uri} -> {StatusCode} (public {PublicStatus}) in {ElapsedMs}ms Correlation={CorrelationId} Body={Body}",
            fault.ClientName,
            fault.Method,
            fault.RequestUri,
            fault.StatusCode,
            fault.PublicStatusCode,
            (int)fault.Duration.TotalMilliseconds,
            fault.CorrelationId,
            fault.ResponseBody);
    }

    private async Task LogTraceBodyAsync(string clientName, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!logger.IsEnabled(LogLevel.Trace))
            return;

        try
        {
            var body = await ReadTruncatedBodyAsync(response, httpOptions.Value.MaxErrorBodyLength, cancellationToken);
            logger.LogTrace("HTTP response body {ClientName}: {Body}", clientName, body);
        }
        catch (Exception ex)
        {
            logger.LogTrace(ex, "Failed to read HTTP response body for {ClientName}", clientName);
        }
    }

    private string ResolveClientName(string clientName)
        => string.IsNullOrWhiteSpace(clientName) ? httpOptions.Value.DefaultClientName : clientName;

    private string? GetRequestCorrelationId(HttpRequestMessage message)
    {
        var headerName = httpOptions.Value.CorrelationHeaderName;
        return message.Headers.TryGetValues(headerName, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    private string? GetCorrelationId(HttpRequestMessage message, HttpResponseMessage response)
    {
        var headerName = httpOptions.Value.CorrelationHeaderName;
        if (response.Headers.TryGetValues(headerName, out var responseValues))
        {
            var fromResponse = responseValues.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(fromResponse))
                return fromResponse;
        }

        return GetRequestCorrelationId(message);
    }

    private static async Task<string?> ReadTruncatedBodyAsync(
        HttpResponseMessage response,
        int maxLength,
        CancellationToken cancellationToken)
    {
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrEmpty(raw) || raw.Length <= maxLength)
            return raw;

        return raw[..maxLength] + "...(truncated)";
    }

    private static Uri BuildRelativeUri(string route, IDictionary<string, string> queryParams)
    {
        if (queryParams is null || queryParams.Count == 0)
            return new Uri(route, UriKind.Relative);

        var query = string.Join("&", queryParams.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        return new Uri($"{route}?{query}", UriKind.Relative);
    }
}
