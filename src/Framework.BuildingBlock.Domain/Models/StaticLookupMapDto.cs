namespace Framework.BuildingBlock.Domain;

public class StaticLookupMapDto
{
    public StaticLookupMapDto()
    {
    }

    public StaticLookupMapDto(int source, int destination)
    {
        this.Source = source;
        this.Destination = destination;
    }

    public int Source { get; set; }
    public int Destination { get; set; }
    public int Code { get; set; }
    public string DestinationType { get; set; }
}