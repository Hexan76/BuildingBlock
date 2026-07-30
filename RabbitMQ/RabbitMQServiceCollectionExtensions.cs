using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.RabbitMQ;

public static class RabbitMQServiceCollectionExtensions
{
    public static IServiceCollection AddFrameworkRabbitMQ(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<RabbitMQOptions>(configuration.GetSection(RabbitMQOptions.SectionName));

        return services.AddFrameworkRabbitMQ();
    }

    public static IServiceCollection AddFrameworkRabbitMQ(
        this IServiceCollection services,
        Action<RabbitMQOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }

        services.AddSingleton<ConsumerRegistry>();
        services.AddSingleton<IRabbitConnectionFactory, RabbitConnectionFactory>();
        services.AddSingleton<ITopologyManager, TopologyManager>();
        services.AddSingleton<IPublishPipeline, PublishPipeline>();
        services.AddSingleton<IConsumePipeline, ConsumePipeline>();
        services.AddTransient<IMessagePublisher, MessagePublisher>();
        services.AddHostedService<RabbitConsumerHostedService>();

        return services;
    }

    public static IServiceCollection AddRabbitConsumer<TConsumer, TMessage>(this IServiceCollection services)
        where TConsumer : class, IMessageConsumer<TMessage>
        where TMessage : class
    {
        services.AddTransient<TConsumer>();
        services.AddSingleton<IConsumerRegistrationContributor, TypedConsumerRegistrationContributor<TConsumer, TMessage>>();
        return services;
    }

    /// <summary>
    /// Registers an RPC request handler that receives <typeparamref name="TRequest"/> and returns
    /// <typeparamref name="TResponse"/>, which is sent back to the caller of
    /// <see cref="IMessagePublisher.RequestAsync{TRequest, TResponse}"/>.
    /// </summary>
    public static IServiceCollection AddRabbitRequestHandler<THandler, TRequest, TResponse>(this IServiceCollection services)
        where THandler : class, IMessageRequestHandler<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        services.AddTransient<THandler>();
        services.AddSingleton<IConsumerRegistrationContributor,
            TypedRequestHandlerRegistrationContributor<THandler, TRequest, TResponse>>();
        return services;
    }

    public static IServiceCollection AddRabbitMQConsumers(this IServiceCollection services, params Assembly[] assemblies)
    {
        var types = assemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .ToArray();

        foreach (var typeInfo in types)
        {
            var consumerInterfaces = typeInfo.ImplementedInterfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageConsumer<>))
                .ToArray();

            foreach (var consumerInterface in consumerInterfaces)
            {
                var messageType = consumerInterface.GetGenericArguments()[0];
                var consumerType = typeInfo.AsType();

                services.AddTransient(consumerType);

                // register a reflection-based contributor instance
                services.AddSingleton<IConsumerRegistrationContributor>(new ReflectionConsumerRegistrationContributor(consumerType, messageType));
            }

            var requestHandlerInterfaces = typeInfo.ImplementedInterfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageRequestHandler<,>))
                .ToArray();

            foreach (var handlerInterface in requestHandlerInterfaces)
            {
                var genericArguments = handlerInterface.GetGenericArguments();
                var requestType = genericArguments[0];
                var responseType = genericArguments[1];
                var handlerType = typeInfo.AsType();

                services.AddTransient(handlerType);

                services.AddSingleton<IConsumerRegistrationContributor>(
                    new ReflectionConsumerRegistrationContributor(handlerType, requestType, responseType));
            }
        }

        return services;
    }

    public static IServiceCollection AddPublishPipeline<TMiddleware>(this IServiceCollection services)
        where TMiddleware : class, IPublishPipelineMiddleware
    {
        services.AddSingleton<IPublishPipelineMiddleware, TMiddleware>();
        return services;
    }

    public static IServiceCollection AddConsumePipeline<TMiddleware>(this IServiceCollection services)
        where TMiddleware : class, IConsumePipelineMiddleware
    {
        services.AddSingleton<IConsumePipelineMiddleware, TMiddleware>();
        return services;
    }
}
