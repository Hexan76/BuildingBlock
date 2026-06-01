using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http;


namespace Framework.BuildingBlock.Application.Contracts;


public class MessageContract<TResponseMessage> : MessageContract
where TResponseMessage : class
{
    public TResponseMessage? Result { get; init; }

    public static MessageContract<TResponseMessage> Success(TResponseMessage data)
    {
        return new()
        {
            Result = data,
            Errors = null,
            Messages = null
        };
    }
}

public class MessageContract
{
    public MessageContract()
    {
        
    }
    public MessageContract(IEnumerable<FrameworkValidation> validations)
    {
        this.Validations = validations;
    }
    public string[]? Errors { get; set; }
    public string[]? Messages { get; set; }
    public MessageContractResultSeverity Severity { get; set; }
    public MessageResultType Type { get; set; }
    public string ApplicationCode { get; set; } = "500";

    [JsonIgnore]
    internal string? InternalStackTrace { get; init; }

    [JsonIgnore]
    internal bool IncludeStackTrace { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StackTrace
        => IncludeStackTrace
            ? InternalStackTrace
            : null;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<FrameworkValidation>? Validations { get; init; }

    public static MessageContract Validation(IEnumerable<FrameworkValidation> validations,HttpContext httpContext)
    {

        return new MessageContract(validations)
        {
            Type = MessageResultType.Error,
            Errors = ["One or more validation errors occurred.", $"Request Path : {httpContext.Request.Path}"],
            InternalStackTrace = $"TraceId :{httpContext.TraceIdentifier}",
        };
    }
}
