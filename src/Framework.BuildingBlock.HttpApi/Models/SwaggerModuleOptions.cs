using NSwag;

namespace Framework.BuildingBlock.HttpApi;

public class SwaggerModuleOptions
{
    public string DocumentName { get; set; } = "API";
    public string Title { get; set; } = "API";
    public string Version { get; set; } = "v1";
    public int ApiVersion { get; set; } = 1;
    public bool ExcludeNonFastEndpoints { get; set; } = true;
    public bool EnableJWTBearerAuth { get; set; } = false;
    public Func<EndpointDefinition, bool> EndpointFilter { get; set; } = _ => true;
    public List<SwaggerHeaderOption>? Headers { get; set; }
    public string? ServerUrl { get; set; }
    public Dictionary<string, OpenApiSecurityScheme> SecurityDefinitions { get; set; } = [];
}
public class SwaggerHeaderOption
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = "";
    public bool Required { get; set; } = false;
}
