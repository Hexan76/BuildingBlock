namespace Framework.BuildingBlock.Data;

/// <summary>
/// Abstraction for applying database schema migrations. It is intentionally free of any
/// specific ORM dependency so it can be injected and used anywhere in the system
/// (API startup, background workers, a dedicated console migrator, tests, etc.).
/// Provide a concrete implementation per persistence technology (e.g. the EF Core
/// implementation in <c>Framework.BuildingBlock.EntityFrameworkCore</c>).
/// </summary>
public interface IDatabaseMigrator
{
    /// <summary>
    /// Applies all pending migrations and then runs the registered
    /// <see cref="IDataSeedContributor"/> instances (if any).
    /// </summary>
    Task MigrateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the migrations that exist in the assembly but have not yet been applied to the database.
    /// </summary>
    Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the migrations that have already been applied to the database.
    /// </summary>
    Task<IReadOnlyList<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default);
}
