using Framework.BuildingBlock.Domain;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock;

[ExposeServices(typeof(IOTPRepository))]
public class OTPRepository(
    IDistributedCache<OTP> cache
) : IOTPRepository, ITransientDependency
{
    public Task<OTP?> GetAsync(string cacheKey)
        => cache.GetAsync(cacheKey);

    public Task SetAsync(string cacheKey, OTP item, TimeSpan ttl)
        => cache.SetAsync(
            cacheKey,
            item,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            });

    public Task RemoveAsync(string cacheKey)
        => cache.RemoveAsync(cacheKey);
}
