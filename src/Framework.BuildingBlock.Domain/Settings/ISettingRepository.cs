using Framework.BuildingBlock.Data;

namespace Framework.BuildingBlock.Settings;

public interface ISettingRepository : IGenericRepository<Setting, Guid>
{
    Task<Setting?> GetAsync(string name, Guid? tenantId, CancellationToken cancellationToken = default);
    Task SetAsync(Setting setting, CancellationToken cancellationToken = default);
}
