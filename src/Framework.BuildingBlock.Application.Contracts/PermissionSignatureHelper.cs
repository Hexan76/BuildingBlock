using System.Security.Cryptography;
using System.Text;

namespace Framework.BuildingBlock.Permissions;

public static class PermissionSignatureHelper
{
    public static string Sign(string payload)
    {
        using var hmac = new HMACSHA256(
            Encoding.UTF8.GetBytes(PermissionSyncConsts.Key));

        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

        return Convert.ToBase64String(hash);
    }
    public static bool Verify(string payload, string signature)
    {
        using var hmac = new HMACSHA256(
            Encoding.UTF8.GetBytes(PermissionSyncConsts.Key));

        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computed = Convert.ToBase64String(hash);

        return computed == signature;
    }
}


