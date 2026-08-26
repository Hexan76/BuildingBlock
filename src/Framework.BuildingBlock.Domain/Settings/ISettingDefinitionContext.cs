namespace Framework.BuildingBlock.Settings;

public interface ISettingDefinitionContext
{
    void Add(SettingDefinition definition);
    SettingDefinition? Get(string name);
    IReadOnlyCollection<SettingDefinition> GetAll();
}
