using System;
using System.Threading.Tasks;

namespace Framework.BuildingBlock.Domain;

public interface ITenantDbNameProvider
{
    Task<string> GetDbName(Guid tenantId);
}
