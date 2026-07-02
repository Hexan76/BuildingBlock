using Framework.BuildingBlock.Domain;

using Volo.Abp.Modularity;

namespace Framework.BuildingBlock;

[DependsOn(
    typeof(BuildingBlockDomainModule),
    typeof(BuildingBlockTestBaseModule)
)]
public class BuildingBlockDomainTestModule : AbpModule
{

}
