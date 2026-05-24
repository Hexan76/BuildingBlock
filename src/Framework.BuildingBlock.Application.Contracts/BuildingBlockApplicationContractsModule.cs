using Framework.BuildingBlock.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace Framework.BuildingBlock.Application.Contracts;

[DependsOn(
    typeof(BuildingBlockDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
    )]
public class BuildingBlockApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IRemoteDispatcher, RemoteDispatcher>();

    }
}
