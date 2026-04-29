using Framework.BuildingBlock.Contracts;
using Framework.BuildingBlock.Domain.Shared;

using HashtApp.Soft.Client.Utilities;

namespace Framework.BuildingBlock.Application;

public static class FilterGroupDtoMap
{
    public static FilterGroup ToDomain(this FilterGroupDto dto)
    {
        if (dto is null)
            return null!;

        return new FilterGroup
        {
            LogicalOperator = (FilterLogicalOperator)(int)dto.LogicalOperator,
            Items = dto.Items?
                .Select(f => f.ToDomain())
                .ToList()
                ?? [],
            SubGroups = dto.SubGroups?
                .Select(f => f.ToDomain())
                .ToList()
                ?? []
        };
    }
    public static FilterItem ToDomain(this FilterRuleDto dto)
    {
        return new FilterItem()
        {
            Property = dto.Property,
            Operator = (FilterOperator)(int)dto.Operator,
            Value = dto.Value
        };
    }
}
