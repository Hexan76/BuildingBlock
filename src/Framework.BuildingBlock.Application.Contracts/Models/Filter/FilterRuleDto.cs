namespace Framework.BuildingBlock.Contracts;

public class FilterRuleDto
{
    public string Property { get; set; } = string.Empty;
    public FilterGroupOperatorDto Operator { get; set; }
    public object? Value { get; set; }
}
