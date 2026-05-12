using Framework.Security;

using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Framework.BuildingBlock.Permission;

public class PermissionDefinitionBuilder
{
    private readonly PermissionDefinitionContext _context;

    public PermissionDefinitionBuilder()
    {
        _context = new PermissionDefinitionContext(null);
    }

    public void Add(PermissionDefinitionDto dto)
    {
        var group = _context.AddGroup(
            dto.GroupName ?? "Default",
            new FixedLocalizableString(dto.GroupName ?? "Default"));

        group.AddPermission(
            dto.Name,
            new FixedLocalizableString(dto.DisplayName ?? dto.Name),
            isEnabled: dto.IsEnabled);
    }

    public IReadOnlyList<PermissionGroupDefinition> GetGroups()
    {
        return _context.Groups.Values.ToList();
    }
}
