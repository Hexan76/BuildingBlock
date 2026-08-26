using System.Reflection;

using Framework.BuildingBlock.Settings;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class FrameworkSettingsServiceCollectionExtensions
{
    public static IServiceCollection AddFrameworkSettings(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        services.Configure<SettingsCacheOptions>(
            configuration.GetSection("Settings:Cache"));
        services.AddSettingDefinitions(assemblies);
        services.AddSettingPersistence();

        return services;
    }

    private static IServiceCollection AddSettingPersistence(
        this IServiceCollection services)
    {
        services.TryAddScoped<ISettingRepository, SettingRepository>();
        services.TryAddScoped<ISettingStore, CachedSettingStore>();
        services.TryAddScoped<ISettings, Settings>();

        return services;
    }
}

internal static class SettingDefinitionRegistrationExtensions
{
    public static IServiceCollection AddSettingDefinitions(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Transient<
                ISettingDefinitionProvider,
                BuildingBlockSettingDefinitionProvider>());

        foreach (var providerType in FindProviders(assemblies))
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient(
                    typeof(ISettingDefinitionProvider),
                    providerType));
        }

        services.TryAddScoped<ISettingDefinitionContext>(
            CreateDefinitionContext);

        return services;
    }

    private static IEnumerable<Type> FindProviders(
        IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(GetLoadableTypes)
            .Where(IsDefinitionProvider)
            .Distinct();
    }

    private static IEnumerable<Type> GetLoadableTypes(
        Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types
                .Where(x => x is not null)!
                .Cast<Type>();
        }
    }

    private static bool IsDefinitionProvider(Type type)
    {
        return typeof(ISettingDefinitionProvider)
            .IsAssignableFrom(type)
            && type is
            {
                IsClass: true,
                IsAbstract: false
            };
    }

    private static ISettingDefinitionContext CreateDefinitionContext(
        IServiceProvider serviceProvider)
    {
        var context = new SettingDefinitionContext();

        foreach (var provider in
                 serviceProvider.GetServices<ISettingDefinitionProvider>())
        {
            provider.Define(context);
        }

        return context;
    }
}
