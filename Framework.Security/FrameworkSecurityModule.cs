
using Framework.Security.Abstraction;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Volo.Abp.Caching;
using Volo.Abp.Modularity;

namespace Framework.Security;

[DependsOn(
    typeof(AbpCachingModule)
)]
public class FrameworkSecurityModule : AbpModule
{
    public override void ConfigureServices(
        ServiceConfigurationContext context)
    {
        var configuration =
            context.Services.GetConfiguration();

        // Options binding
        context.Services.Configure<TokenHttpClientOptions>(
            configuration.GetSection("Framework:Security"));

        // HttpClient
        context.Services.AddHttpClient(
            FrameworkSecurityConstants.HttpClientName,
            client =>
            {
                var options =
                    configuration
                        .GetSection("Framework:Security")
                        .Get<TokenHttpClientOptions>();

                if (options != null)
                {
                    client.BaseAddress =
                        new Uri(options.BaseAddress);
                }
            });

        // Token Provider
        context.Services.AddTransient<
            ITokenProvider,
            OpenIddictTokenProvider>();

        // Delegating Handler
        context.Services.AddTransient<
            ServiceAuthenticationHandler>();
            context.Services.AddHttpClient<
    IPermissionClient,
    PermissionClient>(
    client =>
    {
        client.BaseAddress =
            new Uri(configuration["PermissionService:BaseUrl"]);
    });
    }
}
