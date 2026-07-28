namespace Framework.HttpClient.Http;

public interface IFormContentBuilder
{
    HttpContent Build(object bodyContent, string contentType);
}
