using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Framework.RabbitMQ;

public sealed class MessagePublisher : IMessagePublisher
{
    private readonly IRabbitConnectionFactory _connectionFactory;
    private readonly ITopologyManager _topologyManager;
    private readonly IPublishPipeline _publishPipeline;
    private readonly RabbitMQOptions _options;
    private readonly ILogger<MessagePublisher> _logger;

    public MessagePublisher(
        IRabbitConnectionFactory connectionFactory,
        ITopologyManager topologyManager,
        IPublishPipeline publishPipeline,
        IOptions<RabbitMQOptions> options,
        ILogger<MessagePublisher> logger)
    {
        _connectionFactory = connectionFactory;
        _topologyManager = topologyManager;
        _publishPipeline = publishPipeline;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync<TMessage>(
        Message<TMessage> message,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default)
        where TMessage : class
    {
        var payloadType = message.Payload?.GetType() ?? typeof(TMessage);
        var exchange = options?.Exchange ?? RabbitMetadataResolver.ResolveExchangeName(payloadType, _options);
        var routingKey = options?.RoutingKey ?? RabbitMetadataResolver.ResolveRoutingKey(payloadType);
        var body = JsonSerializer.Serialize(message.Payload);

        var context = new PublishContext
        {
            MessageType = payloadType,
            Message = message,
            Exchange = exchange,
            RoutingKey = routingKey,
            Body = body,
            CancellationToken = cancellationToken
        };

        if (options?.Headers != null)
        {
            foreach (var header in options.Headers)
            {
                context.Headers[header.Key] = header.Value;
            }
        }

        if (message.Headers != null)
        {
            foreach (var h in message.Headers)
            {
                context.Headers[h.Key] = h.Value;
            }
        }

        // Add wrapper metadata into headers for consumers/observability
        context.Headers["x-message-id"] = message.Id.ToString();
        context.Headers["x-message-type"] = message.Type;
        if (message.CorrelationId.HasValue)
        {
            context.Headers["x-correlation-id"] = message.CorrelationId.Value.ToString();
        }

        context.Headers["x-message-timestamp"] = message.Timestamp.ToString("o");

        await _publishPipeline.ExecuteAsync(
            context,
            () => PublishInternalAsync(context, payloadType, cancellationToken),
            cancellationToken);
    }

    private const string DirectReplyToQueue = "amq.rabbitmq.reply-to";

    // Web defaults => camelCase + case-insensitive, which matches JavaScript/NestJS conventions.
    private static readonly JsonSerializerOptions NestJsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<TResponse> RequestAsync<TRequest, TResponse>(
        Message<TRequest> message,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class
    {
        var payloadType = message.Payload?.GetType() ?? typeof(TRequest);
        var exchange = options?.Exchange ?? RabbitMetadataResolver.ResolveExchangeName(payloadType, _options);
        var routingKey = options?.RoutingKey ?? RabbitMetadataResolver.ResolveRoutingKey(payloadType);
        var timeout = options?.Timeout ?? TimeSpan.FromSeconds(30);
        var correlationId = (message.CorrelationId ?? Guid.NewGuid()).ToString("N");
        var bodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message.Payload));

        if (_options.AutoDeclareExchanges && !string.IsNullOrEmpty(exchange))
        {
            var exchangeType = RabbitMetadataResolver.ResolveExchangeType(payloadType, _options);
            var (durable, autoDelete) = RabbitMetadataResolver.ResolveExchangeFlags(payloadType);

            await _topologyManager.DeclareExchangeAsync(
                exchange,
                exchangeType,
                durable,
                autoDelete,
                cancellationToken);
        }

        var responseBytes = await AwaitReplyAsync(
            exchange,
            routingKey,
            correlationId,
            bodyBytes,
            BuildRequestHeaders(message, options, correlationId),
            payloadType.FullName,
            timeout,
            cancellationToken);

        return JsonSerializer.Deserialize<TResponse>(responseBytes)
               ?? throw new InvalidOperationException(
                   $"Failed to deserialize the RPC response to '{typeof(TResponse).Name}'.");
    }

    /// <summary>
    /// Sends a request whose payload is a dynamic/loosely-typed model and waits for the response.
    /// The wire format matches the NestJS microservice envelope:
    /// <c>{"pattern":"...","data":{...},"id":"..."}</c>, and the NestJS reply envelope
    /// (<c>{"response":...,"id":"...","isDisposed":true}</c>) is automatically unwrapped.
    /// </summary>
    /// <remarks>
    /// For NestJS interop set <see cref="PublishOptions.RoutingKey"/> to the target microservice
    /// queue name and leave <see cref="PublishOptions.Exchange"/> empty (default exchange).
    /// </remarks>
    public async Task<TResponse> RequestAsync<TResponse>(
        string pattern,
        object data,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        using var document = await RequestRawAsync(pattern, data, options, cancellationToken);
        return UnwrapResponse<TResponse>(document.RootElement, pattern);
    }

    /// <summary>
    /// Same as <see cref="RequestAsync{TResponse}(string, object, RequestOptions?, CancellationToken)"/>
    /// but returns the response as a raw <see cref="JsonElement"/> for fully dynamic access.
    /// </summary>
    public async Task<JsonElement> RequestAsync(
        string pattern,
        object data,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var document = await RequestRawAsync(pattern, data, options, cancellationToken);
        return ExtractResponseElement(document.RootElement).Clone();
    }

    private async Task<JsonDocument> RequestRawAsync(
        string pattern,
        object data,
        RequestOptions? options,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        // NestJS routes to the microservice queue via the default exchange; the "pattern" lives in
        // the body, not in the routing key. Callers target the queue through options.RoutingKey.
        var exchange = options?.Exchange ?? string.Empty;
        var routingKey = options?.RoutingKey ?? pattern;
        var timeout = options?.Timeout ?? TimeSpan.FromSeconds(30);
        var messageId = Guid.NewGuid().ToString("N");

        var envelope = new NestMessageEnvelope
        {
            Pattern = pattern,
            Data = data,
            Id = messageId
        };

        var bodyBytes = JsonSerializer.SerializeToUtf8Bytes(envelope, NestJsonOptions);

        if (_options.AutoDeclareExchanges && !string.IsNullOrEmpty(exchange))
        {
            await _topologyManager.DeclareExchangeAsync(
                exchange,
                _options.DefaultExchangeType,
                durable: true,
                autoDelete: false,
                cancellationToken);
        }

        var headers = BuildDynamicHeaders(options, messageId);

        var responseBytes = await AwaitReplyAsync(
            exchange,
            routingKey,
            messageId,
            bodyBytes,
            headers,
            pattern,
            timeout,
            cancellationToken);

        return JsonDocument.Parse(responseBytes);
    }

    /// <summary>
    /// Publishes a request over a dedicated channel and asynchronously waits for a single reply,
    /// matched by correlation id, using the built-in "direct reply-to" pseudo queue.
    /// </summary>
    private async Task<byte[]> AwaitReplyAsync(
        string exchange,
        string routingKey,
        string correlationId,
        byte[] body,
        IDictionary<string, object?>? headers,
        string? type,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        // A dedicated channel per request keeps correlation isolated and lets us use the lightweight
        // built-in "direct reply-to" pseudo queue instead of creating/deleting a temporary queue.
        await using var channel = await _connectionFactory.CreateChannelAsync(cancellationToken);

        var responseSource = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, eventArgs) =>
        {
            if (string.Equals(eventArgs.BasicProperties.CorrelationId, correlationId, StringComparison.Ordinal))
            {
                responseSource.TrySetResult(eventArgs.Body.ToArray());
            }

            return Task.CompletedTask;
        };

        // The reply consumer must be listening before we publish the request.
        await channel.BasicConsumeAsync(
            queue: DirectReplyToQueue,
            autoAck: true,
            consumer: consumer,
            cancellationToken: cancellationToken);

        var properties = new BasicProperties
        {
            //ContentType = "application/json",
            //Type = type,
            CorrelationId = correlationId,
            ReplyTo = DirectReplyToQueue,
            Headers = headers
        };

        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogDebug(
            "Sent RPC request to {Exchange} [{RoutingKey}] (correlationId {CorrelationId})",
            exchange,
            routingKey,
            correlationId);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        await using (timeoutCts.Token.Register(
            static state => ((TaskCompletionSource<byte[]>)state!).TrySetCanceled(),
            responseSource))
        {
            try
            {
                return await responseSource.Task;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"RPC request to '{routingKey}' timed out after {timeout.TotalSeconds:0.##}s " +
                    $"(correlationId: {correlationId}).");
            }
        }
    }

    private static TResponse UnwrapResponse<TResponse>(JsonElement root, string pattern)
        where TResponse : class
    {
        var responseElement = ExtractResponseElement(root);

        if (responseElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            throw new InvalidOperationException(
                $"The RPC responder returned an empty response for pattern '{pattern}'.");
        }

        return responseElement.Deserialize<TResponse>(NestJsonOptions)
               ?? throw new InvalidOperationException(
                   $"Failed to deserialize the RPC response to '{typeof(TResponse).Name}'.");
    }

    /// <summary>
    /// Unwraps the NestJS reply envelope (<c>{ response, id, isDisposed, err }</c>).
    /// If the body is not an envelope, the whole payload is returned as-is.
    /// </summary>
    private static JsonElement ExtractResponseElement(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return root;
        }

        if (root.TryGetProperty("err", out var error) &&
            error.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined))
        {
            throw new InvalidOperationException(
                $"The RPC responder returned an error: {error.GetRawText()}");
        }

        return root.TryGetProperty("response", out var response)
            ? response
            : root;
    }

    private static IDictionary<string, object?> BuildDynamicHeaders(
        RequestOptions? options,
        string correlationId)
    {
        var headers = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (options?.Headers != null)
        {
            foreach (var header in options.Headers)
            {
                headers[header.Key] = header.Value;
            }
        }

        headers["x-correlation-id"] = correlationId;

        return headers;
    }

    private static IDictionary<string, object?> BuildRequestHeaders<TRequest>(
        Message<TRequest> message,
        RequestOptions? options,
        string correlationId)
        where TRequest : class
    {
        var headers = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (options?.Headers != null)
        {
            foreach (var header in options.Headers)
            {
                headers[header.Key] = header.Value;
            }
        }

        if (message.Headers != null)
        {
            foreach (var header in message.Headers)
            {
                headers[header.Key] = header.Value;
            }
        }

        headers["x-message-id"] = message.Id.ToString();
        headers["x-message-type"] = message.Type;
        headers["x-correlation-id"] = correlationId;
        headers["x-message-timestamp"] = message.Timestamp.ToString("o");

        return headers;
    }

    private async Task PublishInternalAsync(
        PublishContext context,
        Type messageType,
        CancellationToken cancellationToken)
    {
        if (_options.AutoDeclareExchanges)
        {
            var exchangeType = RabbitMetadataResolver.ResolveExchangeType(messageType, _options);
            var (durable, autoDelete) = RabbitMetadataResolver.ResolveExchangeFlags(messageType);

            await _topologyManager.DeclareExchangeAsync(
                context.Exchange,
                exchangeType,
                durable,
                autoDelete,
                cancellationToken);
        }

        await using var channel = await _connectionFactory.CreateChannelAsync(cancellationToken);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Type = messageType.FullName
        };

        if (context.Headers.Count > 0)
        {
            properties.Headers = context.Headers.ToDictionary(
                pair => pair.Key,
                pair => (object?)pair.Value);
        }

        var bodyBytes = Encoding.UTF8.GetBytes(context.Body);

        await channel.BasicPublishAsync(
            exchange: context.Exchange,
            routingKey: context.RoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: bodyBytes,
            cancellationToken: cancellationToken);

        _logger.LogDebug(
            "Published {MessageType} to {Exchange} [{RoutingKey}]",
            messageType.Name,
            context.Exchange,
            context.RoutingKey);
    }

    /// <summary>
    /// The request envelope understood by NestJS microservices:
    /// <c>{ "pattern": "...", "data": {...}, "id": "..." }</c>.
    /// </summary>
    private sealed class NestMessageEnvelope
    {
        public string Pattern { get; set; } = default!;

        public object? Data { get; set; }

        public string Id { get; set; } = default!;
    }
}
