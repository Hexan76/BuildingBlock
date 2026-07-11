namespace Framework.RabbitMQ;

public class QueueDefinition
{
    public string Name { get; set; } = default!;

    public bool Durable { get; set; } = true;

    public bool Exclusive { get; set; }

    public bool AutoDelete { get; set; }

    public string? Exchange { get; set; }

    public string? RoutingKey { get; set; }
}
