using Microsoft.Extensions.FileProviders;

namespace Framework.Localization;

public abstract class LocalizationResourceBase
{
    public string ResourceName { get; }

    public string PhysicalPath { get; set; }
    public bool IsEmbedded => string.IsNullOrEmpty(PhysicalPath);
    protected LocalizationResourceBase(string resourceName)
    {
        ResourceName = resourceName;
    }
    protected LocalizationResourceBase(string resourceName, string physicalPath)
    {
        ResourceName = resourceName;
        PhysicalPath = physicalPath;
    }
}

public class TypedLocalizationResource : LocalizationResourceBase
{
    public Type ResourceType { get; set; }
    public IFileProvider? FileProvider { get; set; }

    public TypedLocalizationResource(string resourceName, Type type, string physicalPath) : base(resourceName, physicalPath)
    {

        ResourceType = type;

        var providers = new List<IFileProvider>();

        if (!string.IsNullOrWhiteSpace(physicalPath))
        {
            var fullPath = Path.GetFullPath(physicalPath, AppContext.BaseDirectory);
            providers.Add(new PhysicalFileProvider(fullPath));

        }

        var baseNamespace = type.Namespace ?? string.Empty;
        providers.Add(new EmbeddedFileProvider(type.Assembly, $"{baseNamespace}.Localization.Resources"));

        FileProvider = new CompositeFileProvider(providers.ToArray());
    }
}

public class UnTypedLocalizationResource : LocalizationResourceBase
{
    public UnTypedLocalizationResource(string resourceName) : base(resourceName)
    {
    }
    public UnTypedLocalizationResource(string resourceName, string physicalPath) : base(resourceName, physicalPath)
    {
    }
}
