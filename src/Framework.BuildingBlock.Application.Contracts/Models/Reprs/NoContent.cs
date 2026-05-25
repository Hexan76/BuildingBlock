namespace Framework.BuildingBlock.Application.Contracts;

public class NoContent
{
    public static readonly NoContent Value = new NoContent();
    private NoContent() { }
}
