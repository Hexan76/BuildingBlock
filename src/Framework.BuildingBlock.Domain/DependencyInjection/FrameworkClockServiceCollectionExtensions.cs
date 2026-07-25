using Framework.BuildingBlock.Abstracts;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Framework.BuildingBlock.DependencyInjection;

public static class FrameworkClockServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="IClock"/> service and the <see cref="ILazyServiceProvider"/>
    /// used by the framework <c>DbContext</c> to resolve services lazily.
    /// <para>
    /// By default the clock works in UTC. Pass <paramref name="configureOptions"/> to switch
    /// to local time or to customize normalization.
    /// </para>
    /// </summary>
    public static IServiceCollection AddFrameworkClock(
        this IServiceCollection services,
        Action<ClockOptions>? configureOptions = null)
    {
        services.Configure<ClockOptions>(options => options.Kind = DateTimeKind.Utc);

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        services.TryAddSingleton<IClock, Framework.BuildingBlock.Clock.Clock>();

        // One lazy provider per scope, wrapping the scope's service provider.
        services.TryAddScoped<ILazyServiceProvider>(
            provider => new LazyServiceProvider(provider));

        return services;
    }
}
