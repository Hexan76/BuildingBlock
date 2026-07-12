namespace Framework.RabbitMQ;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RabbitExchangeAttribute : Attribute
{
    public string ExchangeName { get; }

    public string ExchangeType { get; set; } = RabbitExchangeTypes.Topic;

    public bool Durable { get; set; } = true;

    public bool AutoDelete { get; set; }

    public RabbitExchangeAttribute(string exchangeName)
    {
        ExchangeName = exchangeName;
    }
}
