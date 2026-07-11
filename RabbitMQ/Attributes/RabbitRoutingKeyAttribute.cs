namespace Framework.RabbitMQ;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RabbitRoutingKeyAttribute : Attribute
{
    public string RoutingKey { get; }

    public RabbitRoutingKeyAttribute(string routingKey)
    {
        RoutingKey = routingKey;
    }
}
