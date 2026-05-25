using Framework.BuildingBlock.Application.Contracts;
using Framework.BuildingBlock.Domain;
using Framework.BuildingBlock.Permission;
using Framework.BuildingBlock.Permissions;
using Framework.Localization;
using Framework.Security;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;

using Volo.Abp.Application;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace Framework.BuildingBlock.Application;

[DependsOn(
    typeof(BuildingBlockDomainModule),
    typeof(BuildingBlockApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(FrameworkSecurityModule),
    typeof(AbpLuckyPennyAutoMapperModule)
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

        var configuration = context.Services.GetConfiguration();

        var isPermissionService =
            configuration.GetValue<bool>("App:SelfPermissionService");

        if (!isPermissionService)
        {
            context.Services.Replace(
                ServiceDescriptor.Transient<
                    IDynamicPermissionDefinitionStore,
                    FrameworkDynamicPermissionDefinitionStore>());

            context.Services.Replace(
                ServiceDescriptor.Transient<
                    IPermissionStore,
                    FrameworkPermissionStore>());
        }

    }
}
