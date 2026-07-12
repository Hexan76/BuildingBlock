namespace Framework.RabbitMQ;

public interface IMessageConsumer<in TMessage>
    where TMessage : class
{
    Task ConsumeAsync(Message<TMessage> message, ConsumeContext context, CancellationToken cancellationToken = default);
}
