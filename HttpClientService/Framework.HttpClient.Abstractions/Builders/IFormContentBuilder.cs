namespace Framework.HttpClient.Http;

public interface IFormContentBuilder
{
    HttpContent Build(IDictionary<string, object> bodyContent, string contentType);
}
