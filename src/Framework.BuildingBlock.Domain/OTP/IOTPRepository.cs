using System;
using System.Threading.Tasks;

namespace Framework.BuildingBlock.Domain;

public interface IOTPRepository
{
    Task<OTP?> GetAsync(string cacheKey);
    Task SetAsync(string cacheKey, OTP item, TimeSpan ttl);
    Task RemoveAsync(string cacheKey);
}

