using System.Net.Http.Headers;
using System.Text;

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

        app.UseScalarBasicAuthentication(options);
        
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
    private static void UseScalarBasicAuthentication(
       this WebApplication app,
       ScalarUiOptions options)
    {
        if (!options.IsBasicAuthenticationEnabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.Username))
        {
            throw new InvalidOperationException(
                "Scalar BasicAuthentication Username is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "Scalar BasicAuthentication Password is not configured.");
        }

        app.Use(async (context, next) =>
        {
            var isScalarRequest =
                context.Request.Path.StartsWithSegments(
                    "/scalar",
                    StringComparison.OrdinalIgnoreCase);

            /*
             * Optional but recommended.
             *
             * Protect OpenAPI documents too.
             */
            var isOpenApiRequest =
                context.Request.Path.StartsWithSegments(
                    "/openapi",
                    StringComparison.OrdinalIgnoreCase);

            if (!isScalarRequest && !isOpenApiRequest)
            {
                await next();
                return;
            }

            if (!context.Request.Headers.TryGetValue(
                    "Authorization",
                    out var authorizationHeader))
            {
                Challenge(context);
                return;
            }

            if (!AuthenticationHeaderValue.TryParse(
                    authorizationHeader,
                    out var authenticationHeader))
            {
                Challenge(context);
                return;
            }

            if (!string.Equals(
                    authenticationHeader.Scheme,
                    "Basic",
                    StringComparison.OrdinalIgnoreCase))
            {
                Challenge(context);
                return;
            }

            if (string.IsNullOrWhiteSpace(
                    authenticationHeader.Parameter))
            {
                Challenge(context);
                return;
            }

            string credentials;

            try
            {
                var credentialBytes =
                    Convert.FromBase64String(
                        authenticationHeader.Parameter);

                credentials =
                    Encoding.UTF8.GetString(
                        credentialBytes);
            }
            catch (FormatException)
            {
                Challenge(context);
                return;
            }

            var separatorIndex =
                credentials.IndexOf(':');

            if (separatorIndex <= 0)
            {
                Challenge(context);
                return;
            }

            var username =
                credentials[..separatorIndex];

            var password =
                credentials[(separatorIndex + 1)..];

            var isValid =
                string.Equals(
                    username,
                    options.Username,
                    StringComparison.Ordinal)
                &&
                string.Equals(
                    password,
                    options.Password,
                    StringComparison.Ordinal);

            if (!isValid)
            {
                Challenge(context);
                return;
            }

            await next();
        });
    }
    private static void Challenge(HttpContext context)
    {
        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        context.Response.Headers.WWWAuthenticate =
            """Basic realm="Scalar API Documentation", charset="UTF-8" """;
    }
}
