namespace Framework.HttpClient.Abstractions
{
    public interface IHttpRequest
    {
        HttpMethod Method { get; }
        string Route { get; set; }
    }
}
