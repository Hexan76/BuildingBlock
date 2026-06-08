using Framework.HttpClient.Abstractions;
using System.Text.Json;

namespace Framework.HttpClient.Http;

public class MessageContractHandler : IResponseHandler<HttpResultModel>
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public MessageContractHandler(JsonSerializerOptions jsonSerializerSettings)
    {
        _jsonSerializerOptions = jsonSerializerSettings;
    }

    public async Task<HttpResultModel> HandleAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<HttpResultModel>(content, _jsonSerializerOptions);
    }

    async Task<object> IResponseHandler.HandleAsync(HttpResponseMessage response, Type targetType)
    {
        return await HandleAsync(response);
    }
}
