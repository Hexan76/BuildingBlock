using Framework.BuildingBlock.Domain.Shared;
using Framework.BuildingBlock.HttpApi;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.ExceptionHandling;

namespace Microsoft.Extensions.DependencyInjection;

public static class BuildingBlockExceptionHandlingExtensions
{
    public static IServiceCollection AddBuildingBlockExceptionHandling(this IServiceCollection services)
    {
        services.Configure<AbpExceptionHandlingOptions>(options =>
        {
            options.SendStackTraceToClients = false;
            options.SendExceptionsDetailsToClients = false;
        });

        services.AddSingleton<FrameworkExceptionMiddlware>();
        services.AddSingleton<IHttpExceptionStatusCodeFinder, FrameworkHttpExceptionStatusCodeFinder>();
        services.AddSingleton<IExceptionToErrorInfoConverter, FrameworkDefaultExceptionToErrorInfoConverter>();
        services.AddSingleton<IExceptionNotifier, ExceptionNotifier>();
        services.AddTransient<IExceptionLocalizationMapper, DefaultExceptionLocalizationMapper>();
        services.AddTransient<LogicalExceptionsHandler>();
        services.AddTransient<DomainExceptionHandler>();
        services.AddTransient<AuthorizationExceptionHandler>();
        services.AddTransient<DefaultExceptionHandler>();
        services.AddSingleton<IEnumerable<IHashtExceptionHandler>>(sp =>
            new IHashtExceptionHandler[]
            {
                sp.GetRequiredService<AuthorizationExceptionHandler>(),
                sp.GetRequiredService<DomainExceptionHandler>(),
                sp.GetRequiredService<LogicalExceptionsHandler>(),
                sp.GetRequiredService<DefaultExceptionHandler>(),
            });

        services.AddLocalization();

        return services;
    }
}
