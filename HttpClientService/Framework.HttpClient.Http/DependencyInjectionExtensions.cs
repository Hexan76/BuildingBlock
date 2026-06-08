using Framework.HttpClient.Abstractions;
using Framework.HttpClient.Http;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static void AddHttpClientFramework(this IServiceCollection services, string baseUrl = "https://api.example.com")
    {
        services.AddTransient<IRequestBuilder, RequestBuilder>();
        services.AddTransient<IRequestResolver, RequestResolver>();
        services.AddTransient<IFormContentBuilder, FormContentBuilder>();
        services.AddTransient<IHttpClientService, HttpClientService>();
        services.AddTransient<IResponseHandlerFactory, ResponseHandlerFactory>();
        services.AddTransient<HttpInterceptorService>();
        services.AddHttpClient("Default", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        })
        .AddHttpMessageHandler<HttpInterceptorService>()
        ;

        services.Configure<HttpClientServiceOptions>(cfg =>
        {
            cfg.JsonSerializerOptions = new()
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
