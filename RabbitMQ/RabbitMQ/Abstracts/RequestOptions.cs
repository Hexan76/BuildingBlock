namespace Framework.RabbitMQ;

/// <summary>
/// Options for an RPC request sent via <see cref="IMessagePublisher.RequestAsync{TRequest, TResponse}"/>.
/// </summary>
public sealed class RequestOptions : PublishOptions
{
    /// <summary>
    /// Maximum time to wait for a response before a <see cref="TimeoutException"/> is thrown.
    /// Defaults to 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
