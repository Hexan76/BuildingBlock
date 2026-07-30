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

    /// <summary>
    /// The reply queue supplied by an RPC requester (RabbitMQ <c>reply-to</c> property), if any.
    /// When set, the message is a request that expects a response.
    /// </summary>
    public string? ReplyTo { get; init; }

    /// <summary>
    /// Correlation id supplied by an RPC requester, echoed back on the response.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// The response produced by an <see cref="IMessageRequestHandler{TRequest, TResponse}"/>.
    /// The consumer host publishes this back to <see cref="ReplyTo"/> when present.
    /// </summary>
    public object? Response { get; set; }

    public CancellationToken CancellationToken { get; init; }
}
