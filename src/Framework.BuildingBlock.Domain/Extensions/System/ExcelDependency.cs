using Framework.BuildingBlock.Domain;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddExcelMapperProfiles(this IServiceCollection services, Assembly assembly)
    {
        var profileTypes = assembly.GetTypes()
            .Where(t => typeof(ExcelMapperProfile).IsAssignableFrom(t)
                        && !t.IsAbstract
                        && t.IsClass);

        foreach (var type in profileTypes)
        {
            services.AddTransient(typeof(ExcelMapperProfile), type);
        }
    }
}
