using Volo.Abp.Reflection;

namespace Framework.BuildingBlock.Permissions;

public class BuildingBlockPermissions
{
    public const string GroupName = "BuildingBlock";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(BuildingBlockPermissions));
    }
}
