using Framework.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization;

namespace Framework.BuildingBlock.Domain.Shared;

[Dependency(ReplaceServices = true)]
public class PlainLocalizableStringSerializer : ILocalizableStringSerializer, ITransientDependency
{
    protected AbpLocalizationOptions LocalizationOptions { get; }

    public PlainLocalizableStringSerializer(IOptions<AbpLocalizationOptions> localizationOptions)
    {
        LocalizationOptions = localizationOptions.Value;
    }

    public string? Serialize(ILocalizableString? localizableString)
    {
        if (localizableString == null)
            return null;

        if (localizableString is LocalizableString realLocalizableString)
        {
            return $"{realLocalizableString.ResourceName},{realLocalizableString.Name}".Replace(':', '.');
        }

        if (localizableString is FixedLocalizableString fixedLocalizableString)
        {
            return $"{fixedLocalizableString.Value}".Replace(':', '.');
        }

        if (localizableString is PlainLocalizableString plain)
            return $"{plain.Name}";

        throw new AbpException($"Unsupported ILocalizableString type: {localizableString.GetType().FullName}");
    }

    public ILocalizableString Deserialize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new AbpException($"{nameof(value)} cannot be null or empty!");

        return new PlainLocalizableString(value);
    }
}
[Dependency(ReplaceServices = true)]
public class PlainLocalizableString : ILocalizableString, IAsyncLocalizableString
{
    public string Name { get; }
    public Type? ResourceType { get; }
    public string? ResourceName { get; }

    public PlainLocalizableString(string name, Type? resourceType = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ResourceType = resourceType;
    }

    public LocalizedString Localize(IStringLocalizerFactory stringLocalizerFactory)
    {
        if (stringLocalizerFactory == null) throw new ArgumentNullException(nameof(stringLocalizerFactory));

        var localizer = stringLocalizerFactory.Create(ResourceType ?? typeof(DefaultResources));
        var result = localizer[Name];
        return new LocalizedString(Name, result.ResourceNotFound ? Name : result.Value);
    }

    public async Task<LocalizedString> LocalizeAsync(IStringLocalizerFactory stringLocalizerFactory)
    {
        if (stringLocalizerFactory == null) throw new ArgumentNullException(nameof(stringLocalizerFactory));

        var localizer = stringLocalizerFactory.Create(ResourceType ?? typeof(DefaultResources));
        var value = localizer[Name];
        return new LocalizedString(Name, value ?? Name);
    }
    public static PlainLocalizableString Create<TResource>(string name)
    {
        return Create(typeof(TResource), name);
    }

    public static PlainLocalizableString Create(Type resourceType, string name)
    {
        return new PlainLocalizableString(name, resourceType);
    }

    public static PlainLocalizableString Create(string name)
    {
        return new PlainLocalizableString(name);
    }
}

