using System.Globalization;
using System.Text.Json;

namespace Framework.BuildingBlock.Settings;

public sealed class Settings : ISettings
{
    private readonly ISettingStore _store;
    private readonly ISettingDefinitionContext _definitions;

    public Settings(ISettingStore store, ISettingDefinitionContext definitions)
    {
        _store = store;
        _definitions = definitions;
    }

    public async Task<T?> GetAsync<T>(string name, CancellationToken cancellationToken = default)
    {
        var definition = GetDefinition(name);
        var value = await _store.GetAsync(name, null, cancellationToken) ?? definition?.DefaultValue;

        return value is null ? default : ConvertValue<T>(value, definition?.ValueType);
    }

    public async Task<T> GetAsync<T>(string name, T defaultValue, CancellationToken cancellationToken = default)
    {
        var value = await GetAsync<T>(name, cancellationToken);
        return value is null ? defaultValue : value;
    }

    public async Task SetAsync<T>(string name, T value, CancellationToken cancellationToken = default)
    {
        var definition = GetDefinition(name);
        var serialized = SerializeValue(value, definition?.ValueType);
        await _store.SetAsync(name, serialized, null, cancellationToken);
    }

    private SettingDefinition? GetDefinition(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Setting name is required.", nameof(name));

        return _definitions.Get(name);
    }

    private static T ConvertValue<T>(string value, SettingValueType? valueType)
    {
        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (targetType == typeof(string))
            return (T)(object)value;

        if (valueType == SettingValueType.Json || targetType != typeof(string) && LooksLikeJson(value))
        {
            var result = JsonSerializer.Deserialize<T>(value);
            return result is null ? default! : result;
        }

        object converted = targetType switch
        {
            var type when type == typeof(int) => int.Parse(value, CultureInfo.InvariantCulture),
            var type when type == typeof(long) => long.Parse(value, CultureInfo.InvariantCulture),
            var type when type == typeof(decimal) => decimal.Parse(value, CultureInfo.InvariantCulture),
            var type when type == typeof(bool) => bool.Parse(value),
            _ => Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture)!
        };

        return (T)converted;
    }

    private static string SerializeValue<T>(T value, SettingValueType? valueType)
    {
        if (value is null)
            return string.Empty;

        if (valueType == SettingValueType.Json || value is not string && value is not ValueType)
            return JsonSerializer.Serialize(value);

        return value switch
        {
            string text => text,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }

    private static bool LooksLikeJson(string value)
    {
        var trimmed = value.TrimStart();
        return trimmed.StartsWith("{") || trimmed.StartsWith("[") || trimmed.StartsWith('"');
    }
}
