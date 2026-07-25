using Framework.BuildingBlock.Domain;

using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Framework.BuildingBlock.EntityFrameworkCore;

[DependsOn(
    typeof(BuildingBlockDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class BuildingBlockEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //context.Services.AddAbpDbContext<BuildingBlockDbContext>(options =>
        //{
        //    options.AddDefaultRepositories(includeAllEntities: true);

        //    /* Add custom repositories here. Example:
        //    * options.AddRepository<Question, EfCoreQuestionRepository>();
        //    */
        //});
    }
}
