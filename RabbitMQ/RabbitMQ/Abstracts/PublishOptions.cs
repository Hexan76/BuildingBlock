namespace Framework.RabbitMQ;

public class PublishOptions
{
    public string? Exchange { get; set; }

    public string? RoutingKey { get; set; }

    public IDictionary<string, object>? Headers { get; set; }
}
