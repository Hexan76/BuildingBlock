namespace Framework.BuildingBlock.Settings;

public sealed class SettingDefinition
{
    public SettingDefinition(string name, SettingValueType valueType = SettingValueType.String, string? defaultValue = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Setting name is required.", nameof(name));

        Name = name;
        ValueType = valueType;
        DefaultValue = defaultValue;
        Description = description;
    }

    public string Name { get; }
    public string? DefaultValue { get; }
    public string? Description { get; }
    public SettingValueType ValueType { get; }
}
