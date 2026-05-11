using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock.Permission;


[Dependency(ReplaceServices = true)]
//[ExposeServices(typeof(IDynamicPermissionDefinitionStore))]
public class MyDynamicPermissionDefinitionStore : IDynamicPermissionDefinitionStore, ITransientDependency
{

    public Task<IReadOnlyList<PermissionGroupDefinition>> GetGroupsAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task<PermissionDefinition?> GetOrNullAsync(string name)
    {
        throw new System.NotImplementedException();
    }

    public Task<IReadOnlyList<PermissionDefinition>> GetPermissionsAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task<PermissionDefinition?> GetResourcePermissionOrNullAsync(string resourceName, string name)
    {
        throw new System.NotImplementedException();
    }

    public Task<IReadOnlyList<PermissionDefinition>> GetResourcePermissionsAsync()
    {
        throw new System.NotImplementedException();
    }
}
