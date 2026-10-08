using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class BuildingBlockCorsExtensions
{
    public const string ConfigurationKey = "App:CorsOrigins";

    public const string EnvironmentVariableName = "App__CorsOrigins";

    private const int MaxIndexedOrigins = 256;

    public static IServiceCollection AddBuildingBlockCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var origins = ResolveCorsOrigins(configuration, environment);

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins(origins)
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static string[] ResolveCorsOrigins(IConfiguration configuration, IHostEnvironment environment)
    {
        var fromEnvironment = ReadEnvironmentOrigins();
        if (fromEnvironment.Length > 0)
        {
            return fromEnvironment;
        }

        if (environment.IsDevelopment())
        {
            return ReadConfigurationOrigins(configuration.GetSection(ConfigurationKey));
        }

        return [];
    }

    private static string[] ReadEnvironmentOrigins()
    {
        var indexed = new List<string>();
        for (var index = 0; index < MaxIndexedOrigins; index++)
        {
            var origin = Environment.GetEnvironmentVariable($"{EnvironmentVariableName}__{index}");
            if (string.IsNullOrWhiteSpace(origin))
            {
                break;
            }

            indexed.Add(origin);
        }

        if (indexed.Count > 0)
        {
            return NormalizeOrigins(indexed);
        }

        var commaSeparated = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (string.IsNullOrWhiteSpace(commaSeparated))
        {
            return [];
        }

        return NormalizeOrigins(
            commaSeparated.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static string[] ReadConfigurationOrigins(IConfigurationSection section)
    {
        if (!section.Exists())
        {
            return [];
        }

        if (!string.IsNullOrWhiteSpace(section.Value))
        {
            return NormalizeOrigins(
                section.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        var indexed = section.GetChildren()
            .OrderBy(child => int.TryParse(child.Key, out var index) ? index : int.MaxValue)
            .Select(child => child.Value);

        return NormalizeOrigins(indexed);
    }

    private static string[] NormalizeOrigins(IEnumerable<string?> origins)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalizedOrigins = new List<string>();

        foreach (var origin in origins)
        {
            if (string.IsNullOrWhiteSpace(origin))
            {
                continue;
            }

            var normalized = origin.Trim().TrimEnd('/');
            if (normalized.Length == 0 || !seen.Add(normalized))
            {
                continue;
            }

            normalizedOrigins.Add(normalized);
        }

        return normalizedOrigins.ToArray();
    }
}
