using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http;


namespace Framework.BuildingBlock.Application.Contracts;


public class MessageContract<TResponseMessage> : MessageContract
where TResponseMessage : class
{
    public TResponseMessage? Result { get; init; }

    public static ApiResult<TResponseMessage> Success(TResponseMessage data)
    {
        return new ApiResult<TResponseMessage>()
        {
            ApplicationCode = "",
            Type = MessageResultType.Info,
            Result = data,
            Errors = null,
            Messages = null
        };
    }
    public static ApiResult<TResponseMessage> Success(TResponseMessage data, string message, int totalCount, int currentPage)
    {
        return new ApiResult<TResponseMessage>()
        {
            Pagination = new()
            {
                CurrentPage = currentPage,
                Total = totalCount
            },
            Snackbar = new()
            {
                Type = MessageContractResultSeverity.Info,
                Message = message
            },
            Error = new()
            {
                Code = 200,
                HttpCode = 200,
                Message = message
            },
            ApplicationCode = "",
            Type = MessageResultType.Info,
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

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StackTrace { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<FrameworkValidation>? Validations { get; init; }

    public static ApiResult Validation(IEnumerable<FrameworkValidation> validations, string applicationCode, HttpContext httpContext)
    {
        string[] errors = ["One or more validation errors occurred.", $"Request Path : {httpContext.Request.Path}"];
        return new ApiResult(validations)
        {
            ApplicationCode = "400",
            Type = MessageResultType.Validation,
            Errors = errors,
            StackTrace = $"TraceId :{httpContext.TraceIdentifier}",
            Error = new()
            {
                Code = int.Parse(applicationCode),
                HttpCode = httpContext.Response.StatusCode,
                Message = string.Join("\n", errors)
            },
            Snackbar = new()
            {
                Type = MessageContractResultSeverity.Error,
                Message = string.Join("\n", validations)
            },
        };
    }
    public static ApiResult ToError(string applicationCode, string[] errors, int statusCode, string stackTrace)
    {
        return new ApiResult
        {
            ApplicationCode = applicationCode,
            Type = MessageResultType.Error,
            Severity = MessageContractResultSeverity.Error,
            Errors = errors,
            Error = new()
            {
                Code = long.Parse(applicationCode),
                HttpCode = statusCode,
                Message = string.Join("\n", errors)
            },
            Snackbar = new()
            {
                Type = MessageContractResultSeverity.Error,
                Message = string.Join("\n", errors)
            },
            StackTrace = stackTrace,
        };
    }
}

//TODO : Handle this because old Requests must be Change NestJs / React and All Services
public class ApiResult<TResponseMessage> : MessageContract<TResponseMessage>
    where TResponseMessage : class
{
    public bool Success { get; set; } = true;
    public Pagination Pagination { get; set; }
    public ErrorDetails? Error { get; set; }
    public Snackbar? Snackbar { get; set; }
}
public class ApiResult : MessageContract
{
    public ApiResult()
    {

    }
    public ApiResult(IEnumerable<FrameworkValidation> validations)
    {
        this.Validations = validations;
    }
    public bool Success { get; set; } = false;
    public Pagination Pagination { get; set; }
    public ErrorDetails? Error { get; set; }
    public Snackbar? Snackbar { get; set; }

}

public class Pagination
{
    public int Total { get; set; }
    public int CurrentPage { get; set; }
}

public class ErrorDetails
{
    public long Code { get; set; }
    public int HttpCode { get; set; }
    public string Message { get; set; }
}

public class Snackbar
{
    public MessageContractResultSeverity Type { get; set; }
    public string Message { get; set; }
}
