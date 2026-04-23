using Framework.BuildingBlock.Domain.Shared;
using Volo.Abp.Application.Services;

namespace Framework.BuildingBlock.Application;

public abstract class BuildingBlockAppService : ApplicationService
{
    protected BuildingBlockAppService()
    {
        LocalizationResource = typeof(BuildingBlockResource);
        ObjectMapperContext = typeof(BuildingBlockApplicationModule);
    }
}
