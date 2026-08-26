using Framework.BuildingBlock.Entities;

namespace Framework.BuildingBlock.Settings;

public sealed class Setting : AuditedAggregateRoot
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
}
