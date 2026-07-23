using Framework.HttpClient.Abstractions;
using Framework.HttpClient.Http;
using Framework.HttpClient.Options;

using Microsoft.Extensions.Configuration;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static void AddHttpClientFramework(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IRequestBuilder, RequestBuilder>();
        services.AddTransient<IRequestResolver, RequestResolver>();
        services.AddTransient<IFormContentBuilder, FormContentBuilder>();
        services.AddTransient<IHttpClientService, HttpClientService>();
        services.AddTransient<IResponseHandlerFactory, ResponseHandlerFactory>();
        services.AddTransient<HttpInterceptorService>();

        var externalServices = configuration
            .GetSection(HttpServicesOptions.SectionName)
            .Get<HttpServicesOptions>();

        if (externalServices?.Services is not null)
        {
            foreach (var service in externalServices.Services)
            {
                services.AddHttpClient(service.Key, client =>
                {
                    client.BaseAddress = new Uri(service.Value.BaseUrl);
                })
                .AddHttpMessageHandler<HttpInterceptorService>();
            }
        }


        services.Configure<HttpClientServiceOptions>(cfg =>
        {
            cfg.JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            cfg.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter());
        });
    }
}
