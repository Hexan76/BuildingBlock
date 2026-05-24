using Framework.BuildingBlock.Contracts;
using System.ComponentModel;

namespace Framework.BuildingBlock.Application.Contracts;


[Serializable]
public class BaseReportListRequest
{
    [DefaultValue(null)]
    public virtual FilterGroupDto FilterGroupDto { get; set; } = null;

    /// <summary>
    /// if ReportId is provided, the report will be generated based on the existing report template with this ID.
    /// else 
    /// the report will be generated based on the default template for the entity of database if not found fallback into static default Report.
    /// </summary>
    public Guid ReportId { get; set; }
    public ICollection<Guid> RecordIds { get; set; } = new List<Guid>();
}
