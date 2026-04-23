using System.Collections.Concurrent;
using System.Reflection;

namespace Framework.Localization;

public class LocalizationResourceDictionary : Dictionary<string, LocalizationResourceBase>
{
    private readonly ConcurrentDictionary<Type, LocalizationResourceBase> _resourceDictionariesByType
        = new ConcurrentDictionary<Type, LocalizationResourceBase>();
    public TypedLocalizationResource Add<TResource>(string? resourceName = null)
    {
        return Add(typeof(TResource), resourceName);
    }
    public TypedLocalizationResource Add<TResource>(string? resourceName = null, string physicalPath = null)
    {
        return Add(typeof(TResource), resourceName, physicalPath);
    }

    public TypedLocalizationResource Add(Type resourceType, string? resourceName = null, string physicalPath = null)
    {
        resourceName ??= GetDefaultNameFromType(resourceType);

        static string GetDefaultNameFromType(Type type)
        {
            var attr = type.GetCustomAttribute<LocalizationResourceAttribute>();
            return attr?.ResourceName ?? type.FullName ?? throw new InvalidOperationException("Cant find a ResourceName for your Class");
        }

        var resource = new TypedLocalizationResource(resourceName, resourceType, physicalPath);
        this[resourceName] = resource;
        _resourceDictionariesByType[resourceType] = resource;
        return resource;
    }

    public UnTypedLocalizationResource Add(string resourceName, string physicalPath)
    {
        if (ContainsKey(resourceName))
        {
            throw new InvalidOperationException($"this Resource has Already been Added: {resourceName}");
        }
        var resource = new UnTypedLocalizationResource(resourceName, physicalPath);
        this[resourceName] = resource;

        return resource;

    }

    public LocalizationResourceBase Get(Type type)
    {
        if (!_resourceDictionariesByType.TryGetValue(type, out var resource))
        {
            throw new KeyNotFoundException($"Resource Key not found for type : {type.FullName}");
        }
        return resource;
    }
}
