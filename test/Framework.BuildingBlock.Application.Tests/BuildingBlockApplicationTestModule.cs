using Framework.BuildingBlock.Application;

using Volo.Abp.Modularity;

namespace Framework.BuildingBlock;

[DependsOn(
    typeof(BuildingBlockApplicationModule),
    typeof(BuildingBlockDomainTestModule)
    )]
public class BuildingBlockApplicationTestModule : AbpModule
{

}
