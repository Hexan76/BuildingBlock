using Framework.HttpClient.Abstractions;

using Microsoft.Extensions.Options;

namespace Framework.HttpClient.Http;

public sealed class HttpClientErrorPolicyRegistry(IOptions<HttpClientErrorPoliciesOptions> options)
    : IHttpClientErrorPolicyRegistry
{
    private static readonly HttpClientErrorPolicy DefaultPolicy = HttpClientErrorPolicy.CreateDefault();

    public HttpClientErrorPolicy Get(string clientName)
    {
        if (!string.IsNullOrWhiteSpace(clientName)
            && options.Value.Policies.TryGetValue(clientName, out var configured))
        {
            return configured;
        }

        return DefaultPolicy;
    }
}
