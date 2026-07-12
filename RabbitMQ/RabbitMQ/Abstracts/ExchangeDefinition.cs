namespace Framework.RabbitMQ;

public class ExchangeDefinition
{
    public string Name { get; set; } = default!;

    public string Type { get; set; } = RabbitExchangeTypes.Topic;

    public bool Durable { get; set; } = true;

    public bool AutoDelete { get; set; }
}
