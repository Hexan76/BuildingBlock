using System;
using System.Runtime.Serialization;
using Volo.Abp.Caching;

namespace Framework.BuildingBlock.Domain;

[CacheName("OTP")]
public class OTP
{
    private const string PrefixKey = "OTP:{0}:{1}";

    public Guid? UserId { get; set; }
    public string? UniqId { get; set; }
    public string RequestType { get; set; } = default!;
    public string HashedCode { get; set; } = default!;
    public long ExpiresAt { get; set; }
    public int Attempts { get; set; }

    [IgnoreDataMember]
    public string? PlainCode { get; set; }

    [IgnoreDataMember]
    public long RemainingSeconds =>
        Math.Max(ExpiresAt - DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 0);

    public static string GetCacheKey(string id, string requestType)
        => string.Format(PrefixKey, id, requestType);
}
