namespace Framework.HttpClient.Abstractions;

public interface IResponseHandlerFactory
{
    IResponseHandler GetHandlerFor(ResponseType responseType = ResponseType.Default, Type typeModel = null);
}
