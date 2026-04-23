using Framework.BuildingBlock.Domain.Shared;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Framework.BuildingBlock.Permissions;

public class BuildingBlockPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(BuildingBlockPermissions.GroupName, L("Permission:BuildingBlock"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BuildingBlockResource>(name);
    }
}
