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
        var section = configuration.GetSection(ConfigurationKey);
        var configuredOrigins = section.GetChildren()
            .OrderBy(child => int.TryParse(child.Key, out var index) ? index : int.MaxValue)
            .Select(child => child.Value);

        if (!configuredOrigins.Any() && !string.IsNullOrWhiteSpace(section.Value))
        {
            configuredOrigins = [section.Value];
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var origins = new List<string>();

        foreach (var configuredOrigin in configuredOrigins)
        {
            if (string.IsNullOrWhiteSpace(configuredOrigin))
            {
                continue;
            }

            var normalized = configuredOrigin.Trim().TrimEnd('/');
            if (normalized.Length == 0 || !seen.Add(normalized))
            {
                continue;
            }

            origins.Add(normalized);
        }

        return origins.ToArray();
    }
}
