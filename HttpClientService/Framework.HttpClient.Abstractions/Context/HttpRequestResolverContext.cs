namespace Framework.HttpClient.Abstractions;

public class HttpRequestResolverContext
{
    public HttpRequestResolverContext(string finalRoute,IDictionary<string, string> queryParams, object? bodyContent)
    {
        FinalRoute = finalRoute;
        QueryParams = queryParams;
        BodyContent = bodyContent;
    }
    public string FinalRoute { get; set; }
    public IDictionary<string, string> QueryParams { get; set; }
    public object BodyContent { get; set; }
}
