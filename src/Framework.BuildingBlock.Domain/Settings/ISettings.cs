namespace Framework.BuildingBlock.Settings;

public interface ISettings
{
    Task<T?> GetAsync<T>(string name, CancellationToken cancellationToken = default);
    Task<T> GetAsync<T>(string name, T defaultValue, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string name, T value, CancellationToken cancellationToken = default);
}
