using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;

namespace Framework.Localization.Extensions;

public static class LocalizationExtensions
{
    public static IServiceCollection RegisterFrameworkLocalization(
        this IServiceCollection services)
    {
        services.Replace(
            ServiceDescriptor.Singleton<
                IStringLocalizerFactory,
                JsonStringLocalizerFactory>());

        services.Replace(
            ServiceDescriptor.Singleton<
                IStringLocalizer,
                JsonStringLocalizer>());

        services.Replace(
            ServiceDescriptor.Singleton<
                IJsonStringLocalizerFactory,
                JsonStringLocalizerFactory>());

        services.Replace(
            ServiceDescriptor.Singleton(
                typeof(IStringLocalizer<>),
                typeof(JsonStringLocalizer<>)));

        return services;
    }
}
