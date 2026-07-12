namespace Framework.RabbitMQ;

public class PublishContext
{
    public required Type MessageType { get; init; }

    public required object Message { get; init; }

    public string Exchange { get; set; } = default!;

    public string RoutingKey { get; set; } = default!;

    public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>();

    public string Body { get; set; } = default!;

    public CancellationToken CancellationToken { get; init; }
}
