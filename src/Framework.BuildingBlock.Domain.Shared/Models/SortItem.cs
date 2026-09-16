namespace Framework.BuildingBlock.Models;

public sealed class SortItem
{
    public string Field { get; init; } = default!;
    public bool Descending { get; init; }
}
