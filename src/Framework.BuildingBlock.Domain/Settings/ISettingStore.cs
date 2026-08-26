namespace Framework.BuildingBlock.Settings;

public interface ISettingStore
{
    Task<string?> GetAsync(string name, Guid? tenantId, CancellationToken cancellationToken = default);
    Task SetAsync(string name, string value, Guid? tenantId, CancellationToken cancellationToken = default);
}
