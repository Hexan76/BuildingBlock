using Framework.BuildingBlock.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Framework.BuildingBlock.EntityFrameworkCore;

public static class DatabaseMigratorServiceCollectionExtensions
{
    /// <summary>
    /// Registers an <see cref="IDatabaseMigrator"/> backed by the given
    /// <typeparamref name="TDbContext"/>. The <c>DbContext</c> itself must already be registered
    /// (e.g. via <c>AddDbContext</c> / <c>AddDbContextFactory</c>).
    /// The migrator is a singleton that creates its own scope per operation, so it can be
    /// resolved and used from anywhere in the system.
    /// </summary>
    public static IServiceCollection AddDatabaseMigrator<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.TryAddSingleton<IDatabaseMigrator, EfCoreDatabaseMigrator<TDbContext>>();
        return services;
    }

    /// <summary>
    /// Registers a data seed contributor that runs after migrations are applied.
    /// Multiple contributors can be registered; they run in registration order.
    /// </summary>
    public static IServiceCollection AddDataSeedContributor<TContributor>(this IServiceCollection services)
        where TContributor : class, IDataSeedContributor
    {
        services.AddScoped<IDataSeedContributor, TContributor>();
        return services;
    }
}
