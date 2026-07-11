using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

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
}
