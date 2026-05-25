using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Volo.Abp.Modularity;

namespace Framework.Localization;

public class LocalizationModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services
            .Replace(ServiceDescriptor.Singleton<IStringLocalizerFactory, JsonStringLocalizerFactory>())
            .Replace(ServiceDescriptor.Scoped<IStringLocalizer, JsonStringLocalizer>())
            .AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>()
            .AddSingleton<IJsonStringLocalizerFactory, JsonStringLocalizerFactory>()
            .AddScoped(typeof(IStringLocalizer<>), typeof(JsonStringLocalizer<>));
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Configure<LocalizationResourceOptions>(options =>
        {
            options.AddResource<DefaultResources>();
        });
    }
}
