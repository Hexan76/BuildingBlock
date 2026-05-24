using Framework.Localization;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Volo.Abp.Security;
using Volo.Abp.Validation;
using Volo.Abp.VirtualFileSystem;

namespace Framework.BuildingBlock.Domain.Shared;

[DependsOn(
    typeof(LocalizationModule),
    typeof(AbpValidationModule),
    typeof(AbpDddDomainSharedModule),
    typeof(AbpSecurityModule),
    typeof(AbpCachingStackExchangeRedisModule)
)]
public class BuildingBlockDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<BuildingBlockDomainSharedModule>();
        });

        Configure<LocalizationResourceOptions>(options =>
        {
            options.AddResource<BuildingBlockResource>();
        });

    }
}
