namespace Framework.Security;

public class PermissionGroupDefinitionDto
{
    public PermissionGroupDefinitionDto(string name, string displayName)
    {
        this.Name = name;
        this.DisplayName = displayName;
    }
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public List<PermissionDefinitionDto> Permissions { get; set; } = new();
}
