namespace Framework.HttpClient.Abstractions;

public interface IResponseHandler
{
    Task<object> HandleAsync(HttpResponseMessage response, Type targetType);
}