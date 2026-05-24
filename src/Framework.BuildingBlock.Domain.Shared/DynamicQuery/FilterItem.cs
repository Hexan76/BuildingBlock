using HashtApp.Soft.Client.Utilities;

namespace Framework.BuildingBlock.Domain.Shared;

public class FilterItem
{
    public string Property { get; set; } = null!;
    public FilterOperator Operator { get; set; }
    public object? Value { get; set; }
}
