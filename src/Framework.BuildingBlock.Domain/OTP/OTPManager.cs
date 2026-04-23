using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock.Domain;

[ExposeServices(typeof(OTPManager))]
public class OTPManager(IOTPRepository otpRepository) : ITransientDependency
{
    private const int MaxAttempts = 3;

    #region helpers

    private static string HashOtp(string otp, string salt)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(
            sha.ComputeHash(Encoding.UTF8.GetBytes(otp + salt)));
    }

    private static string GenerateOtp(int length = 6)
    {
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        return string.Concat(bytes.Select(b => (b % 10).ToString()));
    }

    #endregion

    #region create

    public Task<OTP> CreateOrGetOtpAsync(string uniqId, string requestType, TimeSpan? expiry = null)
        => CreateOrGetInternalAsync(uniqId, requestType, expiry, null);

    public Task<OTP> CreateOrGetOtpAsync(Guid userId, string requestType, TimeSpan? expiry = null)
        => CreateOrGetInternalAsync(userId.ToString(), requestType, expiry, userId);

    private async Task<OTP> CreateOrGetInternalAsync(
        string id,
        string requestType,
        TimeSpan? expiry,
        Guid? userId)
    {
        var ttl = expiry ?? TimeSpan.FromMinutes(2);

        var cacheKey = OTP.GetCacheKey(id, requestType);
        var existing = await otpRepository.GetAsync(cacheKey);

        if (existing != null && existing.RemainingSeconds > 0)
        {
            existing.PlainCode = null;
            var remaining = TimeSpan.FromSeconds(existing.RemainingSeconds);

            existing.Attempts++;
            await otpRepository.SetAsync(cacheKey, existing, remaining);
            return existing;
        }

        var otp = GenerateOtp();

        var item = new OTP
        {
            UserId = userId,
            UniqId = userId == null ? id : null,
            RequestType = requestType,
            HashedCode = HashOtp(otp, id),
            ExpiresAt = DateTimeOffset.UtcNow.Add(ttl).ToUnixTimeSeconds(),
            Attempts = 0
        };

        await otpRepository.SetAsync(cacheKey, item, ttl);

        item.PlainCode = otp;
        return item;
    }

    #endregion

    #region verify

    public Task<bool> VerifyOtpAsync(Guid userId, string requestType, string inputOtp)
        => VerifyInternalAsync(userId.ToString(), requestType, inputOtp);

    public Task<bool> VerifyOtpAsync(string uniqId, string requestType, string inputOtp)
        => VerifyInternalAsync(uniqId, requestType, inputOtp);

    private async Task<bool> VerifyInternalAsync(
        string id,
        string requestType,
        string inputOtp)
    {
        var cacheKey = OTP.GetCacheKey(id, requestType);
        var otpItem = await otpRepository.GetAsync(cacheKey);

        if (otpItem == null || otpItem.RemainingSeconds <= 0)
            return false;

        if (otpItem.Attempts >= MaxAttempts)
            return false;

        var isValid = CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(HashOtp(inputOtp, id)),
            Convert.FromBase64String(otpItem.HashedCode));

        if (!isValid)
        {
            otpItem.Attempts++;
            await otpRepository.SetAsync(
                cacheKey,
                otpItem,
                TimeSpan.FromSeconds(otpItem.RemainingSeconds));

            return false;
        }

        // success → consume OTP
        await otpRepository.RemoveAsync(cacheKey);
        return true;
    }

    #endregion
}
