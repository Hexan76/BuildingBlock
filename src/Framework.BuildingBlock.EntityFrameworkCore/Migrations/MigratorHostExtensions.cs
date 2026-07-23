using Framework.BuildingBlock.Data;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Framework.BuildingBlock.EntityFrameworkCore;

public static class MigratorHostExtensions
{
    /// <summary>
    /// Resolves the registered <see cref="IDatabaseMigrator"/> and applies pending migrations
    /// (and seed contributors). Call this from <c>Program.cs</c> of an API/worker to migrate on
    /// startup, or from a dedicated console migrator.
    /// Requires <c>AddDatabaseMigrator&lt;TDbContext&gt;()</c> to have been called.
    /// </summary>
    public static async Task MigrateDatabaseAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        using var scope = host.Services.CreateScope();
        var migrator = scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>();
        await migrator.MigrateAsync(cancellationToken);
    }
}
