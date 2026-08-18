namespace Framework.BuildingBlock.HttpApi;

[Obsolete("Use OpenApiDefinitionOptions.")]
public class SwaggerModuleOptions : OpenApiDefinitionOptions
{
    public string? ServerUrl
    {
        get => Servers.FirstOrDefault()?.Url;
        set
        {
            Servers.Clear();
            if (!string.IsNullOrWhiteSpace(value))
            {
                Servers.Add(new OpenApiServerOption { Url = value });
            }
        }
    }

    public new List<SwaggerHeaderOption>? Headers
    {
        get => field;
        set
        {
            field = value;
            base.Headers = value is null ? null : [.. value];
        }
    }
}

[Obsolete("Use OpenApiHeaderOption.")]
public class SwaggerHeaderOption : OpenApiHeaderOption;
