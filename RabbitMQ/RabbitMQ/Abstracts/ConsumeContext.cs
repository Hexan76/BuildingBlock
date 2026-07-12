namespace Framework.RabbitMQ;

public class ConsumeContext
{
    public required Type MessageType { get; init; }

    public required object Message { get; init; }

    public required string QueueName { get; init; }

    public required string Exchange { get; init; }

    public required string RoutingKey { get; init; }

    public required ulong DeliveryTag { get; init; }

    public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>();

    public string RawBody { get; init; } = default!;

    public CancellationToken CancellationToken { get; init; }
}
