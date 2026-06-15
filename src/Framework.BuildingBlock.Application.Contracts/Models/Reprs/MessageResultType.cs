using System.Text.Json.Serialization;

namespace Framework.BuildingBlock.Application.Contracts
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageResultType
    {
        Message,
        Error,
        Validation,
    }
}
