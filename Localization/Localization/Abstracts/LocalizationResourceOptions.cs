namespace Framework.Localization;

public class LocalizationResourceOptions
{
    public LocalizationResourceDictionary Resources { get; } = new LocalizationResourceDictionary();
    public string BaseName { get; }
    public string BaseLocation { get; }
    public void AddResource<TResource>(string? resourceName = null)
        where TResource : class
    {
        var type = typeof(TResource);

        Resources.Add<TResource>(resourceName);
    }
    public void AddResource<TResource>(string physicalPath, string? resourceName = null)
        where TResource : class
    {
        var type = typeof(TResource);

        Resources.Add<TResource>(resourceName, physicalPath);
    }
}
