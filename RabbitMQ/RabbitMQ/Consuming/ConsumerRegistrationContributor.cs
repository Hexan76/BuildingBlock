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
            ExchangeName = RabbitMetadataResolver.ResolveExchangeName(messageType, options),
            RoutingKey = RabbitMetadataResolver.ResolveRoutingKey(messageType),
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

    public ReflectionConsumerRegistrationContributor(Type consumerType, Type messageType)
    {
        _consumerType = consumerType;
        _messageType = messageType;
    }

    public ConsumerRegistration CreateRegistration(RabbitMQOptions options)
    {
        var queueFlags = RabbitMetadataResolver.ResolveQueueFlags(_consumerType);

        return new ConsumerRegistration
        {
            ConsumerType = _consumerType,
            MessageType = _messageType,
            QueueName = RabbitMetadataResolver.ResolveQueueName(_consumerType, _messageType),
            ExchangeName = RabbitMetadataResolver.ResolveExchangeName(_messageType, options),
            RoutingKey = RabbitMetadataResolver.ResolveRoutingKey(_messageType),
            Durable = queueFlags.Durable,
            Exclusive = queueFlags.Exclusive,
            AutoDelete = queueFlags.AutoDelete
        };
    }
}
