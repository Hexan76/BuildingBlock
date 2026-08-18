using FastEndpoints.Swagger;

using Framework.BuildingBlock.HttpApi;

using NSwag;

namespace Microsoft.Extensions.DependencyInjection;

public static class OpenApiDocumentExtensions
{
    [Obsolete("Use AddOpenApiDocuments.")]
    public static IServiceCollection FrameworkNSwagDocsPerModule(
        this IServiceCollection services,
        params OpenApiDefinitionOptions[] definitions)
        => services.AddOpenApiDocuments(definitions);

    public static IServiceCollection AddOpenApiDocuments(
        this IServiceCollection services,
        params OpenApiDefinitionOptions[] definitions)
    {
        services.AddSingleton(new OpenApiDocumentsRegistry { Definitions = definitions });

        foreach (var definition in definitions)
        {
            services.SwaggerDocument(c =>
            {
                c.EnableJWTBearerAuth = definition.EnableJWTBearerAuth;
                c.ExcludeNonFastEndpoints = definition.ExcludeNonFastEndpoints;
                c.MinEndpointVersion = definition.ApiVersion;
                c.MaxEndpointVersion = definition.ApiVersion;
                c.EndpointFilter = definition.EndpointFilter;
                c.AutoTagPathSegmentIndex = 0;

                c.DocumentSettings = s =>
                {
                    s.DocumentName = definition.DocumentName;
                    s.Title = definition.Title;
                    s.Version = definition.Version;

                    var servers = definition.Servers
                        .Where(server => !string.IsNullOrWhiteSpace(server.Url))
                        .ToList();

                    if (servers.Count > 0)
                    {
                        s.PostProcess = document => ApplyServers(document, servers);
                    }

                    if (definition.Headers?.Count > 0)
                    {
                        s.OperationProcessors.Add(
                            new AddHeaderOperationProcessor(definition.Headers));
                    }

                    foreach (var item in definition.SecurityDefinitions)
                    {
                        s.AddAuth(item.Key, item.Value);
                        s.AddSecurity(item.Key, Enumerable.Empty<string>(), item.Value);
                    }
                };
            });
        }

        return services;
    }

    internal static void ApplyServers(OpenApiDocument document, IEnumerable<OpenApiServerOption> servers)
    {
        document.Servers.Clear();
        foreach (var server in servers)
        {
            document.Servers.Add(new OpenApiServer
            {
                Url = server.Url,
                Description = server.Description
            });
        }
    }
}
