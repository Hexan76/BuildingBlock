//namespace Framework.BuildingBlock.Domain;

//public static class FilterGroupDtoDtoExtensions
//{
//    // Helper to create a FilterRule
//    public static FilterRuleDto CreateRule(string property, FilterGroupOperatorDto op, object? value = null)
//    {
//        if (string.IsNullOrWhiteSpace(property)) throw new ArgumentException("Property name must be provided.", nameof(property));
//        return new FilterRuleDto
//        {
//            Property = property,
//            Operator = op,
//            Value = value
//        };
//    }

//    // Add an existing rule to the group
//    public static FilterGroupDto AddRule(this FilterGroupDto group, FilterRuleDto rule)
//    {
//        if (group == null) throw new ArgumentNullException(nameof(group));
//        if (rule == null) throw new ArgumentNullException(nameof(rule));
//        group.Items ??= new List<FilterRuleDto>();
//        group.Items.Add(rule);
//        return group;
//    }

//    // Add by parameters (fluent)
//    public static FilterGroupDto AddRule(this FilterGroupDto group, string property, object? value, FilterGroupOperatorDto op = FilterGroupOperatorDto.Equal)
//    {
//        if (group == null) throw new ArgumentNullException(nameof(group));
//        var rule = CreateRule(property, op, value);
//        return group.AddRule(rule);
//    }

//    // Common helpers for readability
//    public static FilterGroupDto WhereEquals(this FilterGroupDto group, string property, object? value) =>
//        group.AddRule(property, value, FilterGroupOperatorDto.Equal);

//    public static FilterGroupDto WhereNotEquals(this FilterGroupDto group, string property, object? value) =>
//        group.AddRule(property, value, FilterGroupOperatorDto.NotEqual);

//    public static FilterGroupDto WhereContains(this FilterGroupDto group, string property, string? value) =>
//        group.AddRule(property, value, FilterGroupOperatorDto.Contains);

//    public static FilterGroupDto WhereStartsWith(this FilterGroupDto group, string property, string? value) =>
//        group.AddRule(property, value, FilterGroupOperatorDto.StartsWith);

//    public static FilterGroupDto WhereEndsWith(this FilterGroupDto group, string property, string? value) =>
//        group.AddRule(property, value, FilterGroupOperatorDto.EndsWith);

//    public static FilterGroupDto WhereGreaterThan(this FilterGroupDto group, string property, object? value) =>
//        group.AddRule(property, value, FilterGroupOperatorDto.GreaterThan);

//    public static FilterGroupDto WhereGreaterThanOrEqual(this FilterGroupDto group, string property, object? value) =>
//        group.AddRule(property, value, FilterGroupOperatorDto.GreaterThanOrEqual);

//    public static FilterGroupDto WhereLessThan(this FilterGroupDto group, string property, object? value) =>
//        group.AddRule(property, value, FilterGroupOperatorDto.LessThan);

//    public static FilterGroupDto WhereLessThanOrEqual(this FilterGroupDto group, string property, object? value) =>
//        group.AddRule(property, value, FilterGroupOperatorDto.LessThanOrEqual);

//    //public static FilterGroupDto WhereIsNull(this FilterGroupDto group, string property) =>
//    //    group.AddRule(property, null, FilterGroupDtoOperator.IsNull);

//    //public static FilterGroupDto WhereIsNotNull(this FilterGroupDto group, string property) =>
//    //    group.AddRule(property, null, FilterGroupDtoOperator.IsNotNull);

//    public static FilterGroupDto WhereIn(this FilterGroupDto group, string property, IEnumerable<object?> values) =>
//        group.AddRule(property, values, FilterGroupOperatorDto.In);

//    // Clear helpers
//    public static FilterGroupDto ClearRules(this FilterGroupDto group)
//    {
//        if (group == null) throw new ArgumentNullException(nameof(group));
//        group.Items?.Clear();
//        group.SubGroups?.Clear();
//        return group;
//    }

//    // Add a subgroup with configuration action (fluent)
//    public static FilterGroupDto AddGroup(this FilterGroupDto parent, Action<FilterGroupDto> configure, FilterLogicalOperatorDto logicalOperator = FilterLogicalOperatorDto.And)
//    {
//        if (parent == null) throw new ArgumentNullException(nameof(parent));
//        if (configure == null) throw new ArgumentNullException(nameof(configure));

//        parent.SubGroups ??= new List<FilterGroupDto>();
//        var subgroup = new FilterGroupDto
//        {
//            LogicalOperator = logicalOperator,
//            Items = new List<FilterRuleDto>(),
//            SubGroups = new List<FilterGroupDto>()
//        };

//        configure(subgroup);

//        parent.SubGroups.Add(subgroup);
//        return parent;
//    }

//    public static FilterGroupDto AddAndGroup(this FilterGroupDto parent, Action<FilterGroupDto> configure) =>
//        parent.AddGroup(configure, FilterLogicalOperatorDto.And);

//    public static FilterGroupDto AddOrGroup(this FilterGroupDto parent, Action<FilterGroupDto> configure) =>
//        parent.AddGroup(configure, FilterLogicalOperatorDto.Or);

//    // Convert a shallow group (items only) to query string using existing ToServerQuery(IEnumerable<FilterRule>)
//    // This is a convenience wrapper if needed elsewhere
//    //public static string ToServerQueryString(this FilterGroupDto group)
//    //{
//    //    if (group == null) return string.Empty;
//    //    return group.Items?.Any() == true ? group.Items.ToServerQuery() : string.Empty;
//    //}
//}
