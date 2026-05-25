using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Framework.Localization;


public class JsonStringLocalizer : IStringLocalizer
{
    private readonly Dictionary<string, Dictionary<string, string>> _cacheDictionary;

    public JsonStringLocalizer()
    {

    }
    public JsonStringLocalizer(Dictionary<string, Dictionary<string, string>> foundedDictionary)
    {
        _cacheDictionary = foundedDictionary;
    }
    public LocalizedString this[string name] => GetStringLocalized(name);

    public LocalizedString this[string name, params object[] arguments] =>
        new LocalizedString(name, string.Format(this[name].Value, arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        foreach (var (key, value) in _cacheDictionary)
        {
            foreach (var item in (key, value).value)
            {
                yield return new LocalizedString(item.Key, item.Value, false);
            }
        }
    }

    private LocalizedString GetStringLocalized(string key)
    {
        return GetStringLocalizedByCulture(key, CultureInfo.CurrentUICulture);
    }
    private LocalizedString GetStringLocalizedByCulture(string key, CultureInfo culture)
    {
        if (!_cacheDictionary.TryGetValue(culture.TwoLetterISOLanguageName, out var resourceDict))
        {
            if (!_cacheDictionary.TryGetValue(CultureInfo.InvariantCulture.TwoLetterISOLanguageName, out var defaultDict))
            {
                return new LocalizedString(key, key);
            }

            return new LocalizedString(key, defaultDict.TryGetValue(key, out var defaultValue) ? defaultValue : key);
        }

        return new LocalizedString(key, resourceDict.TryGetValue(key, out var value) ? value : key);
    }
}

public class JsonStringLocalizer<T> : IStringLocalizer<T>
{
    private readonly IStringLocalizer _localizer;
    public JsonStringLocalizer(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create(typeof(T));
    }

    public LocalizedString this[string name] => _localizer[name];

    public LocalizedString this[string name, params object[] arguments] =>
        new LocalizedString(name, string.Format(_localizer[name].Value, arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        foreach (var localizedString in _localizer.GetAllStrings(includeParentCultures))
        {
            yield return localizedString;
        }
    }
}

