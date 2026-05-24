using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace Framework.Localization;

public class JsonStringLocalizerFactory : IJsonStringLocalizerFactory
{
    private readonly Dictionary<Type, Dictionary<string, Dictionary<string, string>>> _localizedData
        = new Dictionary<Type, Dictionary<string, Dictionary<string, string>>>();

    private readonly LocalizationResourceDictionary _cacheResources;
    private readonly SupportedCulturesLocaliztionOptions _cultures;
    public JsonStringLocalizerFactory(
        IOptions<SupportedCulturesLocaliztionOptions> cultures,
        IOptions<LocalizationResourceOptions> options)
    {
        _cacheResources = options.Value.Resources;
        _cultures = cultures.Value;
        PreLoadDictionary();
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        if (!_localizedData.TryGetValue(resourceSource, out var cultureDict))
        {
            //TODO: Handle the case where the resource is not found
            _localizedData.TryGetValue(typeof(DefaultResources), out var cultureDictDeafult);
            return new JsonStringLocalizer(cultureDictDeafult);
            //throw new InvalidOperationException($"No resource registered for {resourceSource.Name}");
        }

        return new JsonStringLocalizer(cultureDict);
    }

    public IStringLocalizer Create(string baseName, string location)
    {

        //TODO Handle EmbeddedResource and PhysicalResource by options
        var resourceType = _cacheResources.Values
            .OfType<TypedLocalizationResource>()
            .FirstOrDefault(resource => resource.ResourceName == baseName)
            ?.ResourceType;

        if (resourceType == null)
            throw new InvalidOperationException($"No resource type found for {location}.{baseName}");

        return Create(resourceType);
    }

    private void PreLoadDictionary()
    {
        foreach (var resource in _cacheResources.Values.OfType<TypedLocalizationResource>())
        {
            var perCultureDict = new Dictionary<string, Dictionary<string, string>>();

            var provider = resource.FileProvider;
            if (provider == null)
                continue;

            var files = provider.GetDirectoryContents("")
                .Where(f => !f.IsDirectory && f.Name.EndsWith(".json"));

            foreach (var file in files)
            {
                var culture = ExtractCulture(file.Name);
                if (!_cultures.Cultures.TryGetValue(new CultureInfo(culture), out var supportCulture))
                    continue;

                using var stream = file.CreateReadStream();
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                var flat = FlattenJson(json);

                perCultureDict[supportCulture.TwoLetterISOLanguageName] = flat;
            }

            _localizedData[resource.ResourceType] = perCultureDict;
        }
    }

    private static Dictionary<string, string> FlattenJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonFlattener.FlattenJson(doc.RootElement);
    }
    private static string ExtractCulture(string resourceName)
    {
        var parts = resourceName.Split('.').Reverse();
        return parts.Count() >= 2 ? parts.ToArray()[1] : "en";
    }
}
