namespace Framework.HttpClient.Abstractions;
public interface IRequestResolver
{
    HttpRequestResolverContext ResolveRequestFields<TRequest>(TRequest request) 
    where TRequest : IHttpRequest;
}
