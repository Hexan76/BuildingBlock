namespace Framework.Security;

public interface IPermissionClient
{
    Task<List<string>> GetGrantedPermissionsAsync(
        string providerName,
        string providerKey,
        CancellationToken cancellationToken = default);

    Task<List<PermissionDefinitionDto>> GetDefinitionsAsync(
        CancellationToken cancellationToken = default);
}
