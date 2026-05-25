using System;
using System.IO;
using System.Threading.Tasks;

namespace Framework.BuildingBlock.Domain;

public interface IDatabaseBackup
{
    Task StartBackupAsync(Guid tenantId);
    Task<Stream> GetTenantBackup(Guid tenantId);
}
