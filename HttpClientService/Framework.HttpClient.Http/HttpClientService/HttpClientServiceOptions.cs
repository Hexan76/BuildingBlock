using System.Text.Json;

using Framework.HttpClient.Abstractions;

namespace Framework.HttpClient.Http;

public class HttpClientServiceOptions
{
    public string DefaultClientName { get; set; } = "Default";
    public TimeSpan TimedOut { get; set; } = TimeSpan.FromSeconds(30);
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new();
    public int MaxErrorBodyLength { get; set; } = 4096;
    public string CorrelationHeaderName { get; set; } = HttpClientHeaders.CorrelationId;
}
