using Framework.BuildingBlock.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Framework.BuildingBlock.Domain;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(BuildingBlockDomainSharedModule)
)]
public class BuildingBlockDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient(typeof(IExcelImporter<>), typeof(ExcelImporter<>));

        context.Services.AddScoped(sp =>
        {
            var profiles = sp.GetServices<ExcelMapperProfile>();
            return new ExcelMapperConfiguration(profiles);
        });

        context.Services.AddTransient<MapperProfileBuilder>();
        // Add other profiles...
        context.Services.AddScoped<IExcelMapperRegistry, ExcelMapperRegistry>();
    }
}
