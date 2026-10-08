using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class BuildingBlockCorsExtensions
{
    public const string ConfigurationKey = "App:CorsOrigins";

    public static IServiceCollection AddBuildingBlockCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var origins = ResolveCorsOrigins(configuration);

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

    public static string[] ResolveCorsOrigins(IConfiguration configuration)
    {
        var configuredOrigins = configuration[ConfigurationKey];
        if (string.IsNullOrWhiteSpace(configuredOrigins))
        {
            return [];
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var origins = new List<string>();

        foreach (var origin in configuredOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var normalized = origin.TrimEnd('/');
            if (normalized.Length == 0 || !seen.Add(normalized))
            {
                continue;
            }

            origins.Add(normalized);
        }

        return origins.ToArray();
    }
}
