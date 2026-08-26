namespace Framework.BuildingBlock.Settings;

public sealed class SettingDefinitionContext : ISettingDefinitionContext
{
    private readonly Dictionary<string, SettingDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);

    public void Add(SettingDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        _definitions[definition.Name] = definition;
    }

    public SettingDefinition? Get(string name)
        => _definitions.TryGetValue(name, out var definition) ? definition : null;

    public IReadOnlyCollection<SettingDefinition> GetAll()
        => _definitions.Values.ToArray();
}
