using Framework.HttpClient.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Framework.HttpClient.Http;

public sealed class HttpClientServiceBuilder
{
    private readonly IHttpClientBuilder _httpClientBuilder;

    internal HttpClientServiceBuilder(IHttpClientBuilder httpClientBuilder)
    {
        _httpClientBuilder = httpClientBuilder;
    }

    public string ClientName => _httpClientBuilder.Name;

    public IServiceCollection Services => _httpClientBuilder.Services;

    public IHttpClientBuilder HttpClientBuilder => _httpClientBuilder;

    public HttpClientServiceBuilder ConfigureClient(Action<System.Net.Http.HttpClient> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _httpClientBuilder.ConfigureHttpClient(configure);
        return this;
    }

    public HttpClientServiceBuilder AddHttpMessageHandler<THandler>()
        where THandler : DelegatingHandler
    {
        Services.TryAddTransient<THandler>();
        _httpClientBuilder.AddHttpMessageHandler<THandler>();
        return this;
    }

    public HttpClientServiceBuilder AddHttpMessageHandler(
        Func<IServiceProvider, DelegatingHandler> configureHandler)
    {
        ArgumentNullException.ThrowIfNull(configureHandler);
        _httpClientBuilder.AddHttpMessageHandler(configureHandler);
        return this;
    }

    /// <summary>
    /// Overrides the default status-code policy for this named client.
    /// Starts from <see cref="HttpClientErrorPolicy.CreateDefault"/> so later rules win.
    /// </summary>
    public HttpClientServiceBuilder ConfigureErrorPolicy(Action<HttpClientErrorPolicy> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var clientName = ClientName;
        Services.Configure<HttpClientErrorPoliciesOptions>(options =>
        {
            var policy = HttpClientErrorPolicy.CreateDefault();
            configure(policy);
            options.Policies[clientName] = policy;
        });
        return this;
    }

    /// <summary>
    /// Replaces the error policy for this named client entirely.
    /// </summary>
    public HttpClientServiceBuilder ConfigureErrorPolicy(HttpClientErrorPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        var clientName = ClientName;
        Services.Configure<HttpClientErrorPoliciesOptions>(options =>
        {
            options.Policies[clientName] = policy;
        });
        return this;
    }
}
