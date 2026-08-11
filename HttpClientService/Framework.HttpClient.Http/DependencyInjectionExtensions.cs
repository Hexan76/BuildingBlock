using Framework.HttpClient.Abstractions;
using Framework.HttpClient.Http;
using Framework.HttpClient.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers core HttpClient framework services (<see cref="IHttpClientService"/>, builders, handlers).
    /// Named clients are registered separately via <see cref="AddHttpClientService"/>.
    /// </summary>
    public static IServiceCollection AddHttpClientFramework(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<HttpClientServiceOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<HttpServicesOptions>(
            configuration.GetSection(HttpServicesOptions.SectionName));

        RegisterCoreServices(services);

        services.Configure<HttpClientServiceOptions>(cfg =>
        {
            cfg.JsonSerializerOptions = CreateDefaultJsonSerializerOptions();
            configure?.Invoke(cfg);
        });

        return services;
    }

    /// <summary>
    /// Registers core HttpClient framework services without configuration binding.
    /// </summary>
    public static IServiceCollection AddHttpClientFramework(
        this IServiceCollection services,
        Action<HttpClientServiceOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        RegisterCoreServices(services);

        services.Configure<HttpClientServiceOptions>(cfg =>
        {
            cfg.JsonSerializerOptions = CreateDefaultJsonSerializerOptions();
            configure?.Invoke(cfg);
        });

        return services;
    }

    /// <summary>
    /// Registers a named HttpClient from <c>ExternalServices:Services:{clientName}</c>
    /// and returns a builder so the microservice can add per-client <see cref="DelegatingHandler"/>s.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddHttpClientFramework(configuration);
    ///
    /// services.AddHttpClientService("Orders", configuration)
    ///     .AddHttpMessageHandler&lt;ServiceAuthenticationHandler&gt;()
    ///     .ConfigureClient(c =&gt; c.Timeout = TimeSpan.FromSeconds(60));
    ///
    /// services.AddHttpClientService("Payments", configuration)
    ///     .AddHttpMessageHandler&lt;ServiceAuthenticationHandler&gt;()
    ///     .AddHttpMessageHandler&lt;PaymentsCustomHandler&gt;();
    /// </code>
    /// </example>
    public static HttpClientServiceBuilder AddHttpClientService(
        this IServiceCollection services,
        string clientName,
        IConfiguration configuration,
        Action<System.Net.Http.HttpClient>? configureClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientName);

        EnsureCoreServices(services);

        var options = GetClientOptions(configuration, clientName);
        return AddHttpClientService(services, clientName, options, configureClient);
    }

    /// <summary>
    /// Registers a named HttpClient with explicitly configured options
    /// (useful when BaseUrl is not coming from <c>ExternalServices</c>).
    /// </summary>
    public static HttpClientServiceBuilder AddHttpClientService(
        this IServiceCollection services,
        string clientName,
        Action<HttpClientItemServiceOptions> configureOptions,
        Action<System.Net.Http.HttpClient>? configureClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientName);

        EnsureCoreServices(services);

        var options = new HttpClientItemServiceOptions();
        configureOptions(options);

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            throw new InvalidOperationException(
                $"HttpClient '{clientName}' requires a non-empty BaseUrl.");
        }

        return AddHttpClientService(services, clientName, options, configureClient);
    }

    /// <summary>
    /// Registers every client defined under <c>ExternalServices:Services</c>
    /// with the default interceptor handler. Prefer <see cref="AddHttpClientService"/>
    /// when a microservice needs custom per-client handlers.
    /// </summary>
    public static IServiceCollection AddHttpClientServicesFromConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        EnsureCoreServices(services);

        var externalServices = configuration
            .GetSection(HttpServicesOptions.SectionName)
            .Get<HttpServicesOptions>();

        if (externalServices?.Services is null || externalServices.Services.Count == 0)
        {
            return services;
        }

        foreach (var service in externalServices.Services)
        {
            AddHttpClientService(services, service.Key, service.Value, configureClient: null);
        }

        return services;
    }

    private static HttpClientServiceBuilder AddHttpClientService(
        IServiceCollection services,
        string clientName,
        HttpClientItemServiceOptions options,
        Action<System.Net.Http.HttpClient>? configureClient)
    {
        var builder = services.AddHttpClient(clientName, client =>
        {
            ApplyClientOptions(client, options);
            configureClient?.Invoke(client);
        });

        // Framework default interceptor (before microservice-specific handlers).
        builder.AddHttpMessageHandler<RequestInterceptorHandler>();

        return new HttpClientServiceBuilder(builder);
    }

    private static void ApplyClientOptions(System.Net.Http.HttpClient client, HttpClientItemServiceOptions options)
    {
        client.BaseAddress = new Uri(options.BaseUrl);

        if (options.Timeout is { } timeout)
        {
            client.Timeout = timeout;
        }

        foreach (var header in options.DefaultHeaders)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    private static HttpClientItemServiceOptions GetClientOptions(
        IConfiguration configuration,
        string clientName)
    {
        var section = configuration.GetSection(
            $"{HttpServicesOptions.SectionName}:Services:{clientName}");

        var options = section.Get<HttpClientItemServiceOptions>();

        if (options is null || string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            throw new InvalidOperationException(
                $"HttpClient '{clientName}' is not configured. " +
                $"Expected section '{HttpServicesOptions.SectionName}:Services:{clientName}' with a BaseUrl.");
        }

        return options;
    }

    private static void RegisterCoreServices(IServiceCollection services)
    {
        services.TryAddTransient<IRequestBuilder, RequestBuilder>();
        services.TryAddTransient<IRequestResolver, RequestResolver>();
        services.TryAddTransient<IFormContentBuilder, FormContentBuilder>();
        services.TryAddTransient<IHttpClientService, HttpClientService>();
        services.TryAddTransient<IResponseHandlerFactory, ResponseHandlerFactory>();
        services.TryAddTransient<IHttpInterceptorService, HttpInterceptorService>();
        services.TryAddTransient<RequestInterceptorHandler>();
    }

    private static void EnsureCoreServices(IServiceCollection services)
    {
        // Allows AddHttpClientService(...) without a prior AddHttpClientFramework call.
        RegisterCoreServices(services);
    }

    private static JsonSerializerOptions CreateDefaultJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
