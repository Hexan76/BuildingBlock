using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.BuildingBlock.Extensions;

public static class JsonSerializationExtensions
{
    public static IServiceCollection AddFrameworkJson(
        this IServiceCollection services)
    {
        services.Configure<JsonOptions>(options =>
        {
            ConfigureJson(options.SerializerOptions);
        });

        services.Configure<JsonSerializerOptions>(options =>
        {
            ConfigureJson(options);
        });

        return services;
    }


    private static void ConfigureJson(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy =
            JsonNamingPolicy.CamelCase;

        options.PropertyNameCaseInsensitive = true;

        options.Converters.Add(
            new JsonStringEnumConverter());

        options.DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull;
    }
}
