using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Framework.BuildingBlock.Settings;

public sealed class CachedSettingStore : ISettingStore
{
    private readonly ISettingRepository _repository;
    private readonly ISettingDefinitionContext _definitions;
    private readonly IDistributedCache? _cache;
    private readonly SettingsCacheOptions _options;

    public CachedSettingStore(
        ISettingRepository repository,
        ISettingDefinitionContext definitions,
        IOptions<SettingsCacheOptions> options,
        IServiceProvider serviceProvider)
    {
        _repository = repository;
        _definitions = definitions;
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.AppKey))
            throw new ArgumentNullException("AppKey for Cache must be valid Value");

        _cache = serviceProvider.GetService<IDistributedCache>();
    }

    public async Task<string?> GetAsync(
        string name,
        Guid? tenantId,
        CancellationToken cancellationToken = default)
    {
        var key = CreateKey(name, tenantId);
        var value = await TryGetCacheAsync(key, cancellationToken)
            ?? (await _repository.GetAsync(name, tenantId, cancellationToken))?.Value
            ?? _definitions.Get(name)?.DefaultValue;

        if (value is not null)
            await TrySetCacheAsync(key, value, cancellationToken);

        return value;
    }

    public async Task SetAsync(
        string name,
        string value,
        Guid? tenantId,
        CancellationToken cancellationToken = default)
    {
        await TrySetCacheAsync(CreateKey(name, tenantId), value, cancellationToken);

        var current = await _repository.GetAsync(name, tenantId, cancellationToken);
        if (current is null)
        {
            current = new Setting
            {
                Id = Guid.NewGuid(),
                Name = name,
                Value = value,
            };
        }
        else
        {
            current.Value = value;
        }

        await _repository.SetAsync(current, cancellationToken);
    }

    private async Task<string?> TryGetCacheAsync(string key, CancellationToken cancellationToken)
    {
        if (_cache is null)
            return null;

        try
        {
            return await _cache.GetStringAsync(key, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task TrySetCacheAsync(string key, string value, CancellationToken cancellationToken)
    {
        if (_cache is null)
            return;

        try
        {
            await _cache.SetStringAsync(
                key,
                value,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
                        Math.Max(1, _options.ExpirationMinutes))
                },
                cancellationToken);
        }
        catch
        {
        }
    }

    private string CreateKey(string name, Guid? tenantId)
        => tenantId.HasValue
            ? $"framework:{_options.AppKey}:settings:{tenantId.Value:D}:{name}"
            : $"framework:{_options.AppKey}:settings:host:{name}";
}
