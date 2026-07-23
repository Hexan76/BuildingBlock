namespace Framework.BuildingBlock.Data;

/// <summary>
/// Implement this interface to seed data right after the database has been migrated.
/// All registered contributors are executed (in registration order) by the
/// <see cref="IDatabaseMigrator"/> once migrations have been applied. Register them with
/// <c>services.AddDataSeedContributor&lt;TContributor&gt;()</c>.
/// </summary>
public interface IDataSeedContributor
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
