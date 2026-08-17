using FastEndpoints.Swagger;

using Framework.BuildingBlock.HttpApi;

namespace Microsoft.Extensions.DependencyInjection;

public static class FastEndpointsSwaggerExtensions
{
    public static IServiceCollection FrameworkNSwagDocsPerModule(this IServiceCollection services, params SwaggerModuleOptions[] modules)
    {
        foreach (var module in modules)
        {
            services.SwaggerDocument(c =>
            {
                c.EnableJWTBearerAuth = module.EnableJWTBearerAuth;

                c.ExcludeNonFastEndpoints =
                    module.ExcludeNonFastEndpoints;

                // <-- built-in FE version filtering
                c.MinEndpointVersion =
                    module.ApiVersion;

                c.MaxEndpointVersion =
                    module.ApiVersion;

                c.EndpointFilter =
                    module.EndpointFilter;

                c.DocumentSettings = s =>
                {
                    s.DocumentName =
                        module.DocumentName;

                    s.Title =
                        module.Title;

                    s.Version =
                        module.Version;

                    if (!string.IsNullOrWhiteSpace(module.ServerUrl))
                    {
                        s.PostProcess = document =>
                        {
                            document.Servers.Clear();

                            document.Servers.Add(
                                new NSwag.OpenApiServer
                                {
                                    Url = module.ServerUrl
                                });
                        };
                    }

                    if (module.Headers?.Any() == true)
                    {
                        s.OperationProcessors.Add(
                            new AddHeaderOperationProcessor(
                                module.Headers));
                    }

                    foreach (var item in module.SecurityDefinitions)
                    {
                        s.AddAuth(item.Key, item.Value);
                        s.AddSecurity(
                               item.Key,
                               Enumerable.Empty<string>(),
                               item.Value);
                    }
                };

                c.AutoTagPathSegmentIndex = 0;
            });
        }
        return services;
    }
}
