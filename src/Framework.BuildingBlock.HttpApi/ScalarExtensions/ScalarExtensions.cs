using Framework.BuildingBlock.HttpApi;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using Scalar.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class ScalarExtensions
{
    public static WebApplication UseConfiguredScalar(this WebApplication app)
    {
        var definitions = app.Services.GetService<OpenApiDocumentsRegistry>()?.Definitions.ToArray()
            ?? [];

        return app.UseConfiguredScalar(app.Configuration, definitions);
    }

    public static WebApplication UseConfiguredScalar(
        this WebApplication app,
        params OpenApiDefinitionOptions[] definitions)
        => app.UseConfiguredScalar(app.Configuration, definitions);

    public static WebApplication UseConfiguredScalar(
        this WebApplication app,
        IConfiguration configuration,
        params OpenApiDefinitionOptions[] definitions)
    {
        return app.UseConfiguredScalar(options =>
        {
            configuration.GetSection(ScalarUiOptions.SectionName).Bind(options);
            if (definitions is { Length: > 0 })
            {
                options.Definitions = definitions;
            }
        });
    }

    public static WebApplication UseConfiguredScalar(this WebApplication app, Action<ScalarUiOptions> configure)
    {
        var options = new ScalarUiOptions();
        configure(options);

        if (options.Definitions is not { Length: > 0 })
        {
            options.Definitions = app.Services.GetService<OpenApiDocumentsRegistry>()?.Definitions.ToArray()
                ?? [];
        }

        app.UseOpenApi(openApi =>
        {
            openApi.Path = options.OpenApiPath;
            openApi.PostProcess = (document, request) =>
            {
                var definition = MatchDefinition(request, options.Definitions);
                var servers = ResolveServers(definition, options.Servers);
                if (servers.Count == 0)
                {
                    return;
                }

                OpenApiDocumentExtensions.ApplyServers(document, servers);
            };
        });

        app.MapScalarApiReference(scalar =>
        {
            scalar.WithOpenApiRoutePattern(options.OpenApiPath);

            if (!string.IsNullOrWhiteSpace(options.Title))
            {
                scalar.WithTitle(options.Title);
            }
            else if (options.Definitions is { Length: 1 })
            {
                scalar.WithTitle(options.Definitions[0].Title);
            }

            if (!string.IsNullOrWhiteSpace(options.BaseServerUrl))
            {
                scalar.WithBaseServerUrl(options.BaseServerUrl);
            }

            if (options.Definitions.Any(definition => definition.EnableJWTBearerAuth))
            {
                scalar.EnablePersistentAuthentication();
            }

            var uiServers = options.Servers
                .Concat(options.Definitions.SelectMany(definition => definition.Servers))
                .Where(server => !string.IsNullOrWhiteSpace(server.Url))
                .DistinctBy(server => server.Url, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var server in uiServers)
            {
                if (string.IsNullOrWhiteSpace(server.Description))
                {
                    scalar.AddServer(server.Url);
                }
                else
                {
                    scalar.AddServer(server.Url, server.Description);
                }
            }

            var hasDefault = options.Definitions.Any(definition => definition.IsDefault);
            for (var i = 0; i < options.Definitions.Length; i++)
            {
                var definition = options.Definitions[i];
                var isDefault = definition.IsDefault || (!hasDefault && i == 0);
                scalar.AddDocument(definition.DocumentName, definition.Title, options.OpenApiPath, isDefault);
            }

            options.Configure?.Invoke(scalar);
        });

        return app;
    }

    internal static List<OpenApiServerOption> ResolveServers(
        OpenApiDefinitionOptions? definition,
        IEnumerable<OpenApiServerOption> globalServers)
    {
        return (definition?.Servers ?? [])
            .Concat(globalServers)
            .Where(server => !string.IsNullOrWhiteSpace(server.Url))
            .DistinctBy(server => server.Url, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    internal static OpenApiDefinitionOptions? MatchDefinition(
        HttpRequest request,
        IEnumerable<OpenApiDefinitionOptions> definitions)
    {
        var path = request.Path.Value;
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return definitions
            .OrderByDescending(definition => definition.DocumentName.Length)
            .FirstOrDefault(definition =>
                path.Contains(definition.DocumentName, StringComparison.OrdinalIgnoreCase)
                || path.Contains(Uri.EscapeDataString(definition.DocumentName), StringComparison.OrdinalIgnoreCase));
    }
}
