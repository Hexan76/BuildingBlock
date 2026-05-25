namespace Framework.BuildingBlock.Application.Contracts;

public class FrameworkPermissionAttribute : Attribute
{
    public string Name { get; }
    public FrameworkPermissionAttribute(string name)
    {
        Name = name;
    }
}