using NSwag;

namespace Framework.BuildingBlock.HttpApi;

public class OpenApiDefinitionOptions
{
    public string DocumentName { get; set; } = "v1";
    public string Title { get; set; } = "API";
    public string Version { get; set; } = "v1";
    public int ApiVersion { get; set; } = 1;
    public bool ExcludeNonFastEndpoints { get; set; } = true;
    public bool EnableJWTBearerAuth { get; set; } = false;
    public bool IsDefault { get; set; }
    public Func<EndpointDefinition, bool> EndpointFilter { get; set; } = _ => true;
    public List<OpenApiHeaderOption>? Headers { get; set; }
    public List<OpenApiServerOption> Servers { get; set; } = [];
    public Dictionary<string, OpenApiSecurityScheme> SecurityDefinitions { get; set; } = [];
}

public class OpenApiServerOption
{
    public string Url { get; set; } = default!;
    public string? Description { get; set; }
}

public class OpenApiHeaderOption
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = "";
    public bool Required { get; set; }
}

internal sealed class OpenApiDocumentsRegistry
{
    public required IReadOnlyList<OpenApiDefinitionOptions> Definitions { get; init; }
}
