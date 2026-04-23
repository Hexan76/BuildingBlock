using System.Collections.Generic;

namespace Framework.BuildingBlock.Domain.Shared;

public class FilterGroup
{
    public FilterLogicalOperator LogicalOperator { get; set; } = FilterLogicalOperator.And;
    public List<FilterItem> Items { get; set; } = [];
    public List<FilterGroup> SubGroups { get; set; } = [];
    // Items می‌تواند FilterItem یا FilterGroup باشد
}
