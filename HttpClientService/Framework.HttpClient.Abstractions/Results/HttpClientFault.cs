namespace Framework.HttpClient.Abstractions;

public sealed class HttpClientFault
{
    public required string ClientName { get; init; }

    public required HttpMethod Method { get; init; }

    public required string RequestUri { get; init; }

    /// <summary>Status code returned by the upstream service.</summary>
    public required int StatusCode { get; init; }

    /// <summary>Status code this API should expose after applying the client error policy.</summary>
    public required int PublicStatusCode { get; init; }

    public string? ResponseBody { get; init; }

    public string? CorrelationId { get; init; }

    public TimeSpan Duration { get; init; }

    public HttpResultModel? UpstreamResult { get; init; }
}
