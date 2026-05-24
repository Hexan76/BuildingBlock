using System;

namespace Framework.BuildingBlock.Domain.Shared;

[AttributeUsage(AttributeTargets.Field)]
public class StaticLookupAttribute : Attribute
{
    public string DisplayName { set; get; }
    public int Order { set; get; }
    public int CodeMap { set; get; }

    public StaticLookupAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}