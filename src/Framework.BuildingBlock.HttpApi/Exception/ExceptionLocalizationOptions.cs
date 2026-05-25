namespace Framework.BuildingBlock.HttpApi;

public class ExceptionLocalizationOptions
{
    public Type DefaultResourceType { get; set; }
    public Dictionary<string, Type> ExceptionResourceMap { get; set; } = new();
}