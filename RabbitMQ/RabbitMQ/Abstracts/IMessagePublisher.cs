namespace Framework.RabbitMQ;

public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(
        Message<TMessage> message,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default)
        where TMessage : class;
}
