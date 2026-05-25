using System;

namespace Framework.BuildingBlock.Domain.Shared;

[Serializable]
public class QueryParameter
{
    /// <summary>
    /// Type of each item, e.g. "int", "Guid", "string"
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// CSV string for lists, or single value as string
    /// </summary>
    public object Value { get; set; }
}

