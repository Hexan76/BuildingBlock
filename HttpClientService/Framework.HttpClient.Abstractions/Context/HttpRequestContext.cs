namespace Framework.HttpClient.Abstractions;

public class HttpRequestContext
{
    public virtual HttpMethod Method { get; set; } = HttpMethod.Post;
    public virtual string Route { get; set; }

}
