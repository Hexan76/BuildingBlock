namespace Framework.RabbitMQ;

public class ConsumerRegistration
{
    public required Type ConsumerType { get; init; }

    public required Type MessageType { get; init; }

    /// <summary>
    /// When set, the consumer is an <see cref="IMessageRequestHandler{TRequest, TResponse}"/> whose
    /// return value (of this type) is published back to the requester.
    /// </summary>
    public Type? ResponseType { get; init; }

    public bool IsRequestHandler => ResponseType is not null;

    public required string QueueName { get; init; }

    public required string ExchangeName { get; init; }

    public required string RoutingKey { get; init; }

    public bool Durable { get; init; } = true;

    public bool Exclusive { get; init; }

    public bool AutoDelete { get; init; }
}

public class ConsumerRegistry
{
    private readonly List<ConsumerRegistration> _registrations = [];

    public IReadOnlyList<ConsumerRegistration> Registrations => _registrations;

    public void Add(ConsumerRegistration registration)
    {
        _registrations.Add(registration);
    }
}
