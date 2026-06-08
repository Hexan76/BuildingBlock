using Framework.HttpClient.Abstractions;
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
    IRequestResolver requestResolver) : IHttpClientService
{
    public async Task<TResponse> SendAsync<TResponse>(
        IHttpRequest request,
        ResponseType WrapType = ResponseType.Default,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null,
        string contentType = "application/json")
        where TResponse : class
    {
        try
        {
            var client = httpClientFactory.CreateClient(string.IsNullOrWhiteSpace(clientName) ? httpOptions.Value.DefaultClientName : clientName);
            var message = requestBuilder.Build(request, contentType);
            AddCustomHeaders(message, customHeaders);

            var response = await client.SendAsync(message);
            response.EnsureSuccessStatusCode();

            var handler = responseHandlerFactory.GetHandlerFor(WrapType,typeof(TResponse));
            var result = await handler.HandleAsync(response, typeof(TResponse));
            return (TResponse)result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HttpClientService failed to send request");
            throw;
        }
    }

    public async Task<TResponse> SendFormAsync<TResponse>(
        IHttpRequest request,
        ResponseType WrapType = ResponseType.Default,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null)
        where TResponse : class
    {
        var client = httpClientFactory.CreateClient(string.IsNullOrWhiteSpace(clientName) ? httpOptions.Value.DefaultClientName : clientName);
        var httpRequestResolverContext = requestResolver.ResolveRequestFields(request);
        var content = formContentBuilder.Build(httpRequestResolverContext.BodyContent, "multipart/form-data");

        var message = new HttpRequestMessage(request.Method, request.Route) { Content = content };
        AddCustomHeaders(message, customHeaders);

        var response = await client.SendAsync(message);
        response.EnsureSuccessStatusCode();

        var handler = responseHandlerFactory.GetHandlerFor(WrapType, typeof(TResponse));
        var result = await handler.HandleAsync(response, typeof(TResponse));
        return (TResponse)result;
    }

    public void AddCustomHeaders(HttpRequestMessage message, Dictionary<string, string>? customHeaders)
    {
        if (customHeaders != null)
        {
            foreach (var kvp in customHeaders)
                message.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
        }
    }
}
