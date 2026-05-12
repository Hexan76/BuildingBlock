namespace Framework.Security;

public class PermissionDefinitionDto
{
    public PermissionDefinitionDto(string name,string displayName)
    {
        this.Name = name;
        this.DisplayName = displayName;
    }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string GroupName { get; set; }
    public bool IsEnabled { get; set; }

}
