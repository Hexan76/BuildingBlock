using Framework.HttpClient.Abstractions;
using System.Text.Json;

namespace Framework.HttpClient.Http;

public class AcceptedResponseHandler<TResponse> : IResponseHandler<TResponse>
    where TResponse : class
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AcceptedResponseHandler(JsonSerializerOptions jsonSerializerSettings)
    {
        _jsonSerializerOptions = jsonSerializerSettings;
    }
    public async Task<TResponse> HandleAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var wrapped = JsonSerializer.Deserialize<HttpResultModel<TResponse>>(content, _jsonSerializerOptions);
        return wrapped.Result;
    }

    async Task<object> IResponseHandler.HandleAsync(HttpResponseMessage response, Type targetType)
    {
        return await HandleAsync(response);
    }
}
