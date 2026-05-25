using Framework.BuildingBlock.Domain.Shared;
using Volo.Abp.AspNetCore.Mvc;

namespace Framework.BuildingBlock.HttpApi;

public abstract class BuildingBlockController : AbpControllerBase
{
    protected BuildingBlockController()
    {
        LocalizationResource = typeof(BuildingBlockResource);
    }
}
