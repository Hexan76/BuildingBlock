namespace Framework.HttpClient.Abstractions;

public interface IHttpClientErrorPolicyRegistry
{
    HttpClientErrorPolicy Get(string clientName);
}
