using System.Text.Json.Serialization;

namespace Framework.HttpClient.Abstractions
{

    public class HttpResultModel
    {
        public string[]? Errors { get; init; }
        public string[]? Messages { get; init; }
        public HttpResultSeverity Severity { get; init; }
        public HttpResultType Type { get; init; }
        public string ApplicationCode { get; init; } = "500";

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
        public IEnumerable<HttpValidationResultItem>? Validations { get; init; }

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

}
