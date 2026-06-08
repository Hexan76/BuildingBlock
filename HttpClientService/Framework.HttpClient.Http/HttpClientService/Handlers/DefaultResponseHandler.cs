using Framework.HttpClient.Abstractions;
using System.Text.Json;

namespace Framework.HttpClient.Http;

public class DefaultResponseHandler : IResponseHandler
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public DefaultResponseHandler(JsonSerializerOptions jsonSettings)
    {
        _jsonSerializerOptions = jsonSettings;
    }

    public async Task<object> HandleAsync(HttpResponseMessage response, Type targetType)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize(content, targetType, _jsonSerializerOptions)!;
    }
}
