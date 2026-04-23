using FastEndpoints.Swagger;
using Framework.BuildingBlock.HttpApi;

namespace Microsoft.Extensions.DependencyInjection;

public static class FastEndpointsSwaggerExtensions
{
    public static IServiceCollection HashtNSwagDocsPerModule(this IServiceCollection services, params SwaggerModuleOptions[] modules)
    {
        foreach (var module in modules)
        {
            services.SwaggerDocument(c =>
            {
                c.EnableJWTBearerAuth = true;
                c.ExcludeNonFastEndpoints = module.ExcludeNonFastEndpoints;
                c.EndpointFilter = module.EndpointFilter;
                c.DocumentSettings = s =>
                {
                    s.DocumentName = module.DocumentName;
                    s.Title = module.Title;
                    s.Version = module.Version;

                    if (module.Headers?.Any() == true)
                    {
                        s.OperationProcessors.Add(new AddHeaderOperationProcessor(module.Headers));
                    }
                };
                c.AutoTagPathSegmentIndex = 0;
            });
        }

        return services;
    }
}