namespace Framework.RabbitMQ;

/// <summary>
/// Handles an RPC-style request and returns a response that is sent back to the caller.
/// This is the responder side of the request/reply pattern; the requester uses
/// <see cref="IMessagePublisher.RequestAsync{TRequest, TResponse}"/>.
/// </summary>
/// <typeparam name="TRequest">The incoming request payload type.</typeparam>
/// <typeparam name="TResponse">The response payload type returned to the caller.</typeparam>
public interface IMessageRequestHandler<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    Task<TResponse> HandleAsync(
        Message<TRequest> message,
        ConsumeContext context,
        CancellationToken cancellationToken = default);
}
