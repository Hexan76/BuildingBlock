using System.Text.Json.Serialization;

namespace Framework.HttpClient.Abstractions;

public class HttpResultModel
{

    public string[]? Errors { get; set; }
    public string[]? Messages { get; set; }
    public HttpResultSeverity Severity { get; set; }
    public HttpResultType Type { get; set; }
    public string ApplicationCode { get; set; } = "500";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StackTrace { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<HttpValidationResultItem>? Validations { get; init; }


    public bool Success { get; set; } = false;
    public Pagination Pagination { get; set; }
    public ErrorDetails? Error { get; set; }
    public Snackbar? Snackbar { get; set; }

}
public class HttpResultModel<T> : HttpResultModel
{
    public T? Result { get; init; }

    public static HttpResultModel<T> Success(T data)
    {
        return new()
        {
            Result = data,
            Errors = null,
            Messages = null
        };
    }
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
    public HttpResultType Type { get; set; }
    public string Message { get; set; }
}
