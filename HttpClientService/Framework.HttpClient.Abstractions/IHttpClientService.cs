namespace Framework.HttpClient.Abstractions;

public interface IHttpClientService
{
    Task<TResponse> SendAsync<TResponse>(
        IHttpRequest request,
        ResponseType WrapType = ResponseType.Default,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null,
        string contentType = "application/json")
        where TResponse : class;

    Task<TResponse> SendFormAsync<TResponse>(
        IHttpRequest request,
        ResponseType WrapType = ResponseType.Default,
        string clientName = "",
        Dictionary<string, string>? customHeaders = null)
        where TResponse : class;

    void AddCustomHeaders(HttpRequestMessage requestMessage, Dictionary<string, string>? customHeaders);
}
