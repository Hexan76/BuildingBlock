namespace Framework.RabbitMQ;

public interface IConsumerRegistrationContributor
{
    ConsumerRegistration CreateRegistration(RabbitMQOptions options);
}

internal sealed class TypedConsumerRegistrationContributor<TConsumer, TMessage> : IConsumerRegistrationContributor
    where TConsumer : class, IMessageConsumer<TMessage>
    where TMessage : class
{
    public ConsumerRegistration CreateRegistration(RabbitMQOptions options)
    {
        var consumerType = typeof(TConsumer);
        var messageType = typeof(TMessage);
        var queueFlags = RabbitMetadataResolver.ResolveQueueFlags(consumerType);

        return new ConsumerRegistration
        {
            ConsumerType = consumerType,
            MessageType = messageType,
            QueueName = RabbitMetadataResolver.ResolveQueueName(consumerType, messageType),
            ExchangeName = RabbitMetadataResolver.ResolveExchangeName(consumerType, options),
            RoutingKey = RabbitMetadataResolver.ResolveRoutingKey(consumerType),
            Durable = queueFlags.Durable,
            Exclusive = queueFlags.Exclusive,
            AutoDelete = queueFlags.AutoDelete
        };
    }
}

internal sealed class TypedRequestHandlerRegistrationContributor<THandler, TRequest, TResponse>
    : IConsumerRegistrationContributor
    where THandler : class, IMessageRequestHandler<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    public ConsumerRegistration CreateRegistration(RabbitMQOptions options)
    {
        var handlerType = typeof(THandler);
        var messageType = typeof(TRequest);
        var queueFlags = RabbitMetadataResolver.ResolveQueueFlags(handlerType);

        return new ConsumerRegistration
        {
            ConsumerType = handlerType,
            MessageType = messageType,
            ResponseType = typeof(TResponse),
            QueueName = RabbitMetadataResolver.ResolveQueueName(handlerType, messageType),
            ExchangeName = RabbitMetadataResolver.ResolveExchangeName(handlerType, options),
            RoutingKey = RabbitMetadataResolver.ResolveRoutingKey(handlerType),
            Durable = queueFlags.Durable,
            Exclusive = queueFlags.Exclusive,
            AutoDelete = queueFlags.AutoDelete
        };
    }
}

internal sealed class ReflectionConsumerRegistrationContributor : IConsumerRegistrationContributor
{
    private readonly Type _consumerType;
    private readonly Type _messageType;
    private readonly Type? _responseType;

    public ReflectionConsumerRegistrationContributor(Type consumerType, Type messageType, Type? responseType = null)
    {
        _consumerType = consumerType;
        _messageType = messageType;
        _responseType = responseType;
    }

    public ConsumerRegistration CreateRegistration(RabbitMQOptions options)
    {
        var queueFlags = RabbitMetadataResolver.ResolveQueueFlags(_consumerType);

        return new ConsumerRegistration
        {
            ConsumerType = _consumerType,
            MessageType = _messageType,
            ResponseType = _responseType,
            QueueName = RabbitMetadataResolver.ResolveQueueName(_consumerType, _messageType),
            ExchangeName = RabbitMetadataResolver.ResolveExchangeName(_consumerType, options),
            RoutingKey = RabbitMetadataResolver.ResolveRoutingKey(_consumerType),
            Durable = queueFlags.Durable,
            Exclusive = queueFlags.Exclusive,
            AutoDelete = queueFlags.AutoDelete
        };
    }
}
