namespace Framework.BuildingBlock.HttpApi;

public class SwaggerModuleOptions
{
    public string DocumentName { get; set; } = "API";
    public string Title { get; set; } = "API";
    public string Version { get; set; } = "1.0.0";
    public bool ExcludeNonFastEndpoints { get; set; } = true;
    public Func<EndpointDefinition, bool> EndpointFilter { get; set; } = _ => true;
    public List<SwaggerHeaderOption>? Headers { get; set; }

}
public class SwaggerHeaderOption
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = "";
    public bool Required { get; set; } = false;
}
