using Framework.Security.Abstraction;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Security.Extensions;

public static class
    FrameworkSecurityServiceCollectionExtensions
{
    public static IServiceCollection
        AddFrameworkSecurity(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services.Configure<TokenHttpClientOptions>(
            configuration.GetSection(
                "Framework:Security"));

        services.AddHttpClient(
            FrameworkSecurityConstants.HttpClientName,
            client =>
            {
                var options =
                    configuration
                        .GetSection(
                            "Framework:Security")
                        .Get<TokenHttpClientOptions>();

                client.BaseAddress =
                    new Uri(options.BaseAddress);
            });

        services.AddTransient<
            ITokenProvider,
            OpenIddictTokenProvider>();

        services.AddTransient<
            ServiceAuthenticationHandler>();

        return services;
    }
}
