using Scalar.AspNetCore;

namespace Framework.BuildingBlock.HttpApi;

public class ScalarUiOptions
{
    public const string SectionName = "Scalar";
    public const string DefaultOpenApiPath = "/openapi/{documentName}.json";

    public string OpenApiPath { get; set; } = DefaultOpenApiPath;
    public string? Title { get; set; }
    public string? BaseServerUrl { get; set; }
    public List<OpenApiServerOption> Servers { get; set; } = [];
    public OpenApiDefinitionOptions[] Definitions { get; set; } = [];
    public Action<ScalarOptions>? Configure { get; set; }
}
