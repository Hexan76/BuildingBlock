namespace Framework.HttpClient.Abstractions;
public interface IRequestBuilder
{
    HttpRequestMessage Build<TRequest>(TRequest request, string contentType)
        where TRequest : IHttpRequest;
}
