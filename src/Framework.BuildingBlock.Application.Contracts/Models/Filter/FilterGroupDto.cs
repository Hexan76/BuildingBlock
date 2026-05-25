namespace Framework.BuildingBlock.Contracts;

public class FilterGroupDto
{
    public FilterLogicalOperatorDto LogicalOperator { get; set; }
    public List<FilterRuleDto> Items { get; set; } = new List<FilterRuleDto>();
    public List<FilterGroupDto> SubGroups { get; set; } = new List<FilterGroupDto>();
}
