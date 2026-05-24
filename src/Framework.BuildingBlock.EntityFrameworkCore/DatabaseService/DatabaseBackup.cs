using Framework.BuildingBlock.Domain;
using Framework.BuildingBlock.Domain.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock;

public class DatabaseBackup : IDatabaseBackup, IScopedDependency
{
    private readonly BackupOptions _options;
    private readonly ITenantConnectionStringProvider _tenantResolver;
    private readonly ITenantDbNameProvider _tenantDbNameProvider;
    private readonly ILogger<DatabaseBackup> _log;

    public DatabaseBackup(
        IOptions<BackupOptions> options,
        ITenantConnectionStringProvider tenantResolver,
        ILogger<DatabaseBackup> log,
        ITenantDbNameProvider tenantDbNameProvider)
    {
        _options = options.Value;
        _tenantResolver = tenantResolver;
        _log = log;
        _tenantDbNameProvider = tenantDbNameProvider;
    }

    public async Task<Stream> GetTenantBackup(Guid tenantId)
    {
        var databaseName = await _tenantDbNameProvider.GetDbName(tenantId);

        var backupFilePath = Path.Combine(_options.RootPath, $"{tenantId}_{databaseName}.bak");

        if (!File.Exists(backupFilePath))
        {
            throw new BusinessException(null, "Backup.NotFound")
                .WithResource(typeof(BuildingBlockResource));
        }

        var memoryStream = new MemoryStream();

        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry($"{databaseName}.bak", CompressionLevel.Optimal);

            await using var entryStream = entry.Open();
            await using var fileStream = new FileStream(
                backupFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                true);

            await fileStream.CopyToAsync(entryStream);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }


    public async Task StartBackupAsync(Guid tenantId)
    {
        var connectionString = await _tenantResolver.GetConnectionStringAsync(tenantId);

        var databaseName = await _tenantDbNameProvider.GetDbName(tenantId);

        var backupFilePath = Path.Combine(_options.RootPath, $"{tenantId}_{databaseName}.bak");

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var backupSql = $@"
            IF EXISTS (SELECT 1 FROM sys.master_files WHERE name = N'{databaseName}')
            BEGIN
                BACKUP DATABASE [{databaseName}]
                TO DISK = N'{backupFilePath}'
                WITH INIT, FORMAT{(_options.UseCompression ? ", COMPRESSION" : "")}
            END
        ";

        using var cmd = new SqlCommand(backupSql, connection);

        try
        {
            await cmd.ExecuteNonQueryAsync();
            _log.LogInformation("Backup completed for database {Database}", databaseName);
        }
        catch (SqlException ex)
        {
            _log.LogError(ex, "Backup failed for database {Database}", databaseName);
            throw;
        }
    }

}
