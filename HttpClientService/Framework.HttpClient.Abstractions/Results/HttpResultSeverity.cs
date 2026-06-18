using System.Text.Json.Serialization;

namespace Framework.HttpClient.Abstractions
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HttpResultSeverity
    {
        Info,
        Warning,
        Error
    }
}
