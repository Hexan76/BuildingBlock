namespace Framework.RabbitMQ;

public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(
        Message<TMessage> message,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Sends a request and asynchronously waits for a single response (RPC / request-reply pattern).
    /// The response is produced by an <see cref="IMessageRequestHandler{TRequest, TResponse}"/> on the
    /// consumer side. Throws <see cref="TimeoutException"/> if no response arrives within
    /// <see cref="RequestOptions.Timeout"/>.
    /// </summary>
    Task<TResponse> RequestAsync<TRequest, TResponse>(
        Message<TRequest> message,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class;

    /// <summary>
    /// Sends a request with a dynamic/loosely-typed payload and waits for the response, deserialized
    /// to <typeparamref name="TResponse"/>. The message is wire-compatible with the NestJS microservice
    /// envelope <c>{"pattern":"...","data":{...},"id":"..."}</c>, and the NestJS reply envelope is
    /// automatically unwrapped. For NestJS interop, set <see cref="PublishOptions.RoutingKey"/> to the
    /// target microservice queue name and leave <see cref="PublishOptions.Exchange"/> empty.
    /// </summary>
    Task<TResponse> RequestAsync<TResponse>(
        string pattern,
        object data,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where TResponse : class;

    /// <summary>
    /// Same as <see cref="RequestAsync{TResponse}(string, object, RequestOptions?, CancellationToken)"/>
    /// but returns the response as a raw <see cref="System.Text.Json.JsonElement"/> for fully dynamic access.
    /// </summary>
    Task<System.Text.Json.JsonElement> RequestAsync(
        string pattern,
        object data,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default);
}
