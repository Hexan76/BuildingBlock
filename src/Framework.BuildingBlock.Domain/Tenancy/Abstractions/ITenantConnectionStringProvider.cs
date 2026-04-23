using System;
using System.Threading.Tasks;

namespace Framework.BuildingBlock.Domain;

public interface ITenantConnectionStringProvider
{
    Task<string> GetConnectionStringAsync(Guid tenantId);
}
