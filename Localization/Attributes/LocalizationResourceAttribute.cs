namespace Framework.Localization;

[AttributeUsage(AttributeTargets.Class)]
public class LocalizationResourceAttribute : Attribute
{
    public string ResourceName { get; }

    public LocalizationResourceAttribute()
    {

    }
    public LocalizationResourceAttribute(string name)
    {
        ResourceName = name;
    }
}
