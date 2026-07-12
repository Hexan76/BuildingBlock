namespace Framework.RabbitMQ;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RabbitQueueAttribute : Attribute
{
    public string QueueName { get; }

    public bool Durable { get; set; } = true;

    public bool Exclusive { get; set; }

    public bool AutoDelete { get; set; }

    public RabbitQueueAttribute(string queueName)
    {
        QueueName = queueName;
    }
}
