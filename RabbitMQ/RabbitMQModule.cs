using Microsoft.Extensions.DependencyInjection;

using Volo.Abp.Modularity;

namespace Framework.RabbitMQ;

public class RabbitMQModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        context.Services.AddFrameworkRabbitMQ(configuration);
    }
}
