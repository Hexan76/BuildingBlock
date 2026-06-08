using System.Text.Json;

namespace Framework.HttpClient.Http;

public class HttpClientServiceOptions
{
    public string DefaultClientName { get; set; } = "Default";
    public TimeSpan TimedOut { get; set; } = TimeSpan.FromSeconds(30);
    public JsonSerializerOptions JsonSerializerOptions { get; set; }
}