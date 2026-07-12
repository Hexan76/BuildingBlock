namespace Framework.RabbitMQ;

public class RabbitMQOptions
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string VirtualHost { get; set; } = "/";

    public bool AutoDeclareQueues { get; set; } = true;

    public bool AutoDeclareExchanges { get; set; } = true;

    public string DefaultExchange { get; set; } = "framework.events";

    public string DefaultExchangeType { get; set; } = RabbitExchangeTypes.Topic;

    public List<ExchangeDefinition> Exchanges { get; } = [];

    public List<QueueDefinition> Queues { get; } = [];

    public void AddExchange(string name, string type = RabbitExchangeTypes.Topic, bool durable = true, bool autoDelete = false)
    {
        Exchanges.Add(new ExchangeDefinition
        {
            Name = name,
            Type = type,
            Durable = durable,
            AutoDelete = autoDelete
        });
    }

    public void AddQueue(
        string name,
        string? exchange = null,
        string? routingKey = null,
        bool durable = true,
        bool exclusive = false,
        bool autoDelete = false)
    {
        Queues.Add(new QueueDefinition
        {
            Name = name,
            Exchange = exchange,
            RoutingKey = routingKey,
            Durable = durable,
            Exclusive = exclusive,
            AutoDelete = autoDelete
        });
    }
}
