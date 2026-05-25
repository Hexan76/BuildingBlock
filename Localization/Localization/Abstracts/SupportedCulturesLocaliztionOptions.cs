using System.Globalization;

namespace Framework.Localization;

public class SupportedCulturesLocaliztionOptions
{
    public HashSet<CultureInfo> Cultures { get; } = new HashSet<CultureInfo>();

    public void AddCulture(string cultureName)
    {
        var culture = new CultureInfo(cultureName);
        Cultures.Add(culture);
    }
}
