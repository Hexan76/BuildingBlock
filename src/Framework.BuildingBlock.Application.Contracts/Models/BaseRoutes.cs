namespace Framework.BuildingBlock.Application.Contracts;

public abstract class BaseRoutes : BaseNoneRoute
{
    protected BaseRoutes(string Prefix, string routeBase) : base(Prefix, routeBase)
    {

    }
    public string Details => $"{Default}/details/{{Id}}";
    public string Get => $"{Default}/{{Id}}";
    public string Delete => $"{Default}/{{Id}}";
    public string Update => $"{Default}/{{Id}}";
    public string GetPaginated => $"{Default}/paginated";
}
public abstract class BaseGetRoute : BaseNoneRoute
{
    public string Get => $"{Default}/{{Id}}";
    public string GetPaginated => $"{Default}/paginated";

    public BaseGetRoute(string Prefix, string routeBase) : base(Prefix, routeBase)
    {
    }
}
public abstract class BaseExcelRoute : BaseRoutes
{
    public string Excel => $"{Default}/excel";
    public string ImportExcel => $"{Excel}/import";
    public string ExportExcel => $"{Excel}/export";

    public BaseExcelRoute(string Prefix, string routeBase) : base(Prefix, routeBase)
    {
    }
}
public abstract class BaseReportRoute : BaseExcelRoute
{
    public string ReportList => $"{Default}/report/list";
    public string Report => $"{Default}/report";

    public BaseReportRoute(string Prefix, string routeBase) : base(Prefix, routeBase)
    {
    }
}

public abstract class BaseNoneRoute
{
    private readonly string prefix;
    private readonly string routeBase;

    public BaseNoneRoute(string Prefix, string routeBase)
    {
        prefix = Prefix;
        this.routeBase = routeBase;
    }

    public string Default => $"{prefix}/{routeBase}";
}
