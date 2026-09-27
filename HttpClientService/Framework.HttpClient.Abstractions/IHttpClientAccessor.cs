namespace Framework.HttpClient.Abstractions;

public interface IHttpClientAccessor
{
    IHttpClientService For(string clientName);
}
