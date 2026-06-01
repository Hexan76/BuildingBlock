using System.Text.Json.Serialization;

namespace Framework.BuildingBlock.Application.Contracts
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageResultType
    {
        Validation,
        Message,
        Error
    }
}
