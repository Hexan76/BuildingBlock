namespace Framework.RabbitMQ;

public interface ITopologyManager
{
    Task DeclareExchangeAsync(
        string name,
        string type,
        bool durable = true,
        bool autoDelete = false,
        CancellationToken cancellationToken = default);

    Task DeclareQueueAsync(
        string name,
        bool durable = true,
        bool exclusive = false,
        bool autoDelete = false,
        CancellationToken cancellationToken = default);

    Task BindQueueAsync(
        string queueName,
        string exchangeName,
        string routingKey,
        CancellationToken cancellationToken = default);

    Task EnsureManualTopologyAsync(CancellationToken cancellationToken = default);
}
