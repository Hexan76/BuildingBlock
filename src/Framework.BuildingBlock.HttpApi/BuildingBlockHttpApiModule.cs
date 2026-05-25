using Framework.BuildingBlock.Application.Contracts;
using Framework.BuildingBlock.Domain.Shared;
using Framework.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace Framework.BuildingBlock.HttpApi;

[DependsOn(
    typeof(BuildingBlockApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
public class BuildingBlockHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(BuildingBlockHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {

        Configure<ExceptionLocalizationOptions>(options =>
        {
            options.DefaultResourceType = typeof(DefaultResources);
            options.ExceptionResourceMap[nameof(BusinessException)] = typeof(DefaultResources);
            options.ExceptionResourceMap[nameof(UserFriendlyException)] = typeof(DefaultResources);
            options.ExceptionResourceMap[nameof(AuthenticationException)] = typeof(DefaultResources);
        });
        context.Services.AddTransient<IExceptionLocalizationMapper, DefaultExceptionLocalizationMapper>();

    }
}
