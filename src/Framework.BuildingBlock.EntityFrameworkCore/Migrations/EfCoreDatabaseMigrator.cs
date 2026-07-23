using Framework.BuildingBlock.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Framework.BuildingBlock.EntityFrameworkCore;

/// <summary>
/// EF Core based implementation of <see cref="IDatabaseMigrator"/> for a specific
/// <typeparamref name="TDbContext"/>. It creates its own service scope for every operation,
/// so it is safe to resolve as a singleton and to use from any host (web app, console, worker).
/// After applying the migrations it runs every registered <see cref="IDataSeedContributor"/>.
/// </summary>
public sealed class EfCoreDatabaseMigrator<TDbContext> : IDatabaseMigrator
    where TDbContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EfCoreDatabaseMigrator<TDbContext>> _logger;

    public EfCoreDatabaseMigrator(
        IServiceScopeFactory scopeFactory,
        ILogger<EfCoreDatabaseMigrator<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        var database = dbContext.Database.GetDbConnection().Database;
        _logger.LogInformation(
            "Starting database migration for {DbContext} (database: {Database})...",
            typeof(TDbContext).Name, database);

        var pending = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count == 0)
        {
            _logger.LogInformation("No pending migrations. The schema is already up to date.");
        }
        else
        {
            _logger.LogInformation(
                "Applying {Count} pending migration(s): {Migrations}",
                pending.Count, string.Join(", ", pending));

            await dbContext.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation("Database migration completed successfully.");
        }

        await RunSeedContributorsAsync(scope.ServiceProvider, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
    }

    public async Task<IReadOnlyList<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return (await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken)).ToList();
    }

    private async Task RunSeedContributorsAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var contributors = provider.GetServices<IDataSeedContributor>().ToList();
        if (contributors.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Running {Count} data seed contributor(s)...", contributors.Count);

        foreach (var contributor in contributors)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Seeding via {Contributor}...", contributor.GetType().Name);
            await contributor.SeedAsync(cancellationToken);
        }

        _logger.LogInformation("Data seeding completed.");
    }
}
