namespace Framework.Security;

public interface IPermissionClient
{
    Task<List<string>> GetGrantedPermissionsAsync(
        string providerName,
        string providerKey);
}
