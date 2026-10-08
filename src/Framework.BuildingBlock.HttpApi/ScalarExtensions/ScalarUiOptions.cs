using Scalar.AspNetCore;

namespace Framework.BuildingBlock.HttpApi;

public class ScalarUiOptions
{
    public const string SectionName = "Scalar";
    public const string DefaultOpenApiPath = "/openapi/{documentName}.json";

    public string OpenApiPath { get; set; } = DefaultOpenApiPath;
    public string? Title { get; set; }
    /// <summary>
    /// Optional Scalar/OpenAPI display version. When empty, the host assembly version is used.
    /// Per-document <see cref="OpenApiDefinitionOptions.DisplayVersion"/> takes precedence.
    /// </summary>
    public string? DisplayVersion { get; set; }
    public string? BaseServerUrl { get; set; }
    public List<OpenApiServerOption> Servers { get; set; } = [];
    public OpenApiDefinitionOptions[] Definitions { get; set; } = [];
    public Action<ScalarOptions>? Configure { get; set; }
    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool IsBasicAuthenticationEnabled =>
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password);
}
