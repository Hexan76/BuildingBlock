namespace Framework.HttpClient.Abstractions;
public interface IResponseHandler<TResponse> : IResponseHandler
{
    Task<TResponse> HandleAsync(HttpResponseMessage response);
}
