using Framework.BuildingBlock.Application.Contracts;
using Framework.BuildingBlock.Domain;
using Framework.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace Framework.BuildingBlock.Application;

[DependsOn(
    typeof(BuildingBlockDomainModule),
    typeof(BuildingBlockApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule)
    )]
public class BuildingBlockApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<BuildingBlockApplicationModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<BuildingBlockApplicationModule>(validate: true);
        });

        context.Services
                .Replace(ServiceDescriptor.Singleton<IStringLocalizerFactory, JsonStringLocalizerFactory>())
                .Replace(ServiceDescriptor.Scoped<IStringLocalizer, JsonStringLocalizer>())
                .AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>()
                .AddSingleton<IJsonStringLocalizerFactory, JsonStringLocalizerFactory>()
                .AddScoped(typeof(IStringLocalizer<>), typeof(JsonStringLocalizer<>));
    }
}
