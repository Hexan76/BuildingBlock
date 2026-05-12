namespace Framework.BuildingBlock.Application.Contracts;

public class FrameworkPermissionModel
{
    public string Group { get; set; } = default!;
    public List<string> Permissions { get; set; } = new();
}
public class PermissionSyncSnapshot
{
    public string ServiceName { get; set; } = default!;
    public List<FrameworkPermissionModel> Groups { get; set; } = new();
}
