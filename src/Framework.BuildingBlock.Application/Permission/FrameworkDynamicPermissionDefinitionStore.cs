using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization;

using Framework.Security;

namespace Framework.BuildingBlock.Permissions;

[Dependency(ReplaceServices = false, TryRegister = false)]
public class FrameworkDynamicPermissionDefinitionStore : IDynamicPermissionDefinitionStore
{
    protected IPermissionClient PermissionClient { get; }

    public FrameworkDynamicPermissionDefinitionStore(IPermissionClient permissionClient)
    {
        PermissionClient = permissionClient;
    }

    public async Task<PermissionDefinition?> GetOrNullAsync(string name)
    {
        var all = await GetPermissionsAsync();
        return all.FirstOrDefault(x => x.Name == name);
    }

    public async Task<IReadOnlyList<PermissionDefinition>> GetPermissionsAsync()
    {
        var remote = await PermissionClient.GetDefinitionsAsync();

        if (remote == null || remote.Count == 0)
        {
            return Array.Empty<PermissionDefinition>();
        }

        var context = new PermissionDefinitionContext(null);

        foreach (var item in remote)
        {
            var group = context.GetGroupOrNull(item.GroupName ?? "Default")
                        ?? context.AddGroup(
                            item.GroupName ?? "Default",
                            new FixedLocalizableString(item.GroupName ?? "Default"));

            group.AddPermission(
                item.Name,
                new FixedLocalizableString(item.DisplayName ?? item.Name),
                isEnabled: item.IsEnabled);
        }

        return context.ResourcePermissions;
    }

    public async Task<IReadOnlyList<PermissionGroupDefinition>> GetGroupsAsync()
    {
        var context = new PermissionDefinitionContext(null);

        var remote = await PermissionClient.GetDefinitionsAsync();

        if (remote == null || remote.Count == 0)
        {
            return Array.Empty<PermissionGroupDefinition>();
        }

        foreach (var item in remote)
        {
            var group = context.GetGroupOrNull(item.GroupName ?? "Default")
                        ?? context.AddGroup(
                            item.GroupName ?? "Default",
                            new FixedLocalizableString(item.GroupName ?? "Default"));

            group.AddPermission(
                item.Name,
                new FixedLocalizableString(item.DisplayName ?? item.Name),
                isEnabled: item.IsEnabled);
        }

        return context.Groups.Values.ToList();
    }

    public async Task<IReadOnlyList<PermissionDefinition>> GetResourcePermissionsAsync()
        => await GetPermissionsAsync();

    public async Task<PermissionDefinition?> GetResourcePermissionOrNullAsync(
        string resourceName,
        string name)
        => await GetOrNullAsync(name);
}
