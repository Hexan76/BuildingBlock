namespace Framework.HttpClient.Abstractions;

public sealed class HttpClientErrorPoliciesOptions
{
    public Dictionary<string, HttpClientErrorPolicy> Policies { get; } = new(StringComparer.OrdinalIgnoreCase);
}
