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
            options.SerializerOptions.PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase;

            options.SerializerOptions.PropertyNameCaseInsensitive = true;

            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter()
            );

            options.SerializerOptions.DefaultIgnoreCondition =
                JsonIgnoreCondition.WhenWritingNull;
        });

        return services;
    }
}
