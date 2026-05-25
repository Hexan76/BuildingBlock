namespace Framework.BuildingBlock.Application.Contracts;

public abstract class PagedAndResultRequestFramework
{
    public virtual int Page { get; set; }
    public virtual int PageSize { get; set; }
}
public abstract class PagedAndSortedResultRequestFramework : PagedAndResultRequestFramework
{
    public string Sort { get; set; }
}
