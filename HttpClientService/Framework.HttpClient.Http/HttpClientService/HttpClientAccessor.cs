using Framework.HttpClient.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.HttpClient.Http;

public sealed class HttpClientAccessor(IServiceProvider serviceProvider, IHttpClientService fallback)
    : IHttpClientAccessor
{
    public IHttpClientService For(string clientName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientName);

        var keyed = serviceProvider.GetKeyedService<IHttpClientService>(clientName);
        return keyed ?? new BoundHttpClientService(fallback, clientName);
    }
}
