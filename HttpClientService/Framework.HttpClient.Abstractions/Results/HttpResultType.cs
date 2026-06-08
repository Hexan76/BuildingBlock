using System.Text.Json.Serialization;

namespace Framework.HttpClient.Abstractions
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HttpResultType
    {
        Validation,
        Message,
        Error,
    }
}
