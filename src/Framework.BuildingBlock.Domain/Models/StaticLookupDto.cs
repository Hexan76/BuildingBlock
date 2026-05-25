namespace Framework.BuildingBlock.Domain;

public class StaticLookupDto
{
    public string Type { get; set; }
    public int Code { get; set; }
    public string DisplayName { get; set; }
    public int SortOrder { get; set; }
    public int? CodeMap { get; set; }
    public string MemberName { get; set; }
}