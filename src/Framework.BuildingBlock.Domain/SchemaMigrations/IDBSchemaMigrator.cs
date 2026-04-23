using System.Threading.Tasks;

namespace Framework.BuildingBlock.Domain;

public interface IDbSchemaMigrator
{
    Task MigrateAsync();
    Task MigrateAsync(string targetMigration);
}
public interface IHostDbSchemaMigrator : IDbSchemaMigrator
{

}
public interface IDbTenantSchemaMigrator : IDbSchemaMigrator
{
    Task MigrateSingleTenant();

}
