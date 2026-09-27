using Framework.HttpClient.Abstractions;

namespace Framework.HttpClient.Http;

public sealed class BoundHttpClientService(IHttpClientService inner, string clientName) : IHttpClientService
{
    public string ClientName { get; } = clientName;

    public Task<TResponse> SendAsync<TResponse>(
        IHttpRequest request,
        ResponseType WrapType = ResponseType.Default,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null,
        string contentType = "application/json")
        where TResponse : class
        => inner.SendAsync<TResponse>(request, WrapType, ClientName, customHeaders, contentType);

    public Task<TResponse> SendAsync<TResponse>(
        IHttpRequest request,
        CancellationToken cancellationToken,
        ResponseType WrapType = ResponseType.Default,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null,
        string contentType = "application/json")
        where TResponse : class
        => inner.SendAsync<TResponse>(request, cancellationToken, WrapType, ClientName, customHeaders, contentType);

    public Task<TResponse> SendFormAsync<TResponse>(
        IHttpRequest request,
        ResponseType WrapType = ResponseType.Default,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null)
        where TResponse : class
        => inner.SendFormAsync<TResponse>(request, WrapType, ClientName, customHeaders);

    public Task<TResponse> SendFormAsync<TResponse>(
        IHttpRequest request,
        CancellationToken cancellationToken,
        ResponseType WrapType = ResponseType.Default,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null)
        where TResponse : class
        => inner.SendFormAsync<TResponse>(request, cancellationToken, WrapType, ClientName, customHeaders);

    public void AddCustomHeaders(HttpRequestMessage requestMessage, Dictionary<string, string>? customHeaders)
        => inner.AddCustomHeaders(requestMessage, customHeaders);
}
