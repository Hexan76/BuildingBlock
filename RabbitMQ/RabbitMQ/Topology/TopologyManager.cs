using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Framework.RabbitMQ;

public sealed class TopologyManager : ITopologyManager
{
    private readonly IRabbitConnectionFactory _connectionFactory;
    private readonly RabbitMQOptions _options;
    private readonly ILogger<TopologyManager> _logger;

    public TopologyManager(
        IRabbitConnectionFactory connectionFactory,
        IOptions<RabbitMQOptions> options,
        ILogger<TopologyManager> logger)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnsureManualTopologyAsync(CancellationToken cancellationToken = default)
    {
        foreach (var exchange in _options.Exchanges)
        {
            await DeclareExchangeAsync(
                exchange.Name,
                exchange.Type,
                exchange.Durable,
                exchange.AutoDelete,
                cancellationToken);
        }

        foreach (var queue in _options.Queues)
        {
            await DeclareQueueAsync(
                queue.Name,
                queue.Durable,
                queue.Exclusive,
                queue.AutoDelete,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(queue.Exchange))
            {
                await BindQueueAsync(
                    queue.Name,
                    queue.Exchange,
                    queue.RoutingKey ?? string.Empty,
                    cancellationToken);
            }
        }
    }

    public async Task DeclareExchangeAsync(
        string name,
        string type,
        bool durable = true,
        bool autoDelete = false,
        CancellationToken cancellationToken = default)
    {
        await using var channel = await _connectionFactory.CreateChannelAsync(cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: name,
            type: type,
            durable: durable,
            autoDelete: autoDelete,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Exchange declared: {Exchange} ({Type})", name, type);
    }

    public async Task DeclareQueueAsync(
        string name,
        bool durable = true,
        bool exclusive = false,
        bool autoDelete = false,
        CancellationToken cancellationToken = default)
    {
        await using var channel = await _connectionFactory.CreateChannelAsync(cancellationToken);

        await channel.QueueDeclareAsync(
            queue: name,
            durable: durable,
            exclusive: exclusive,
            autoDelete: autoDelete,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Queue declared: {Queue}", name);
    }

    public async Task BindQueueAsync(
        string queueName,
        string exchangeName,
        string routingKey,
        CancellationToken cancellationToken = default)
    {
        await using var channel = await _connectionFactory.CreateChannelAsync(cancellationToken);

            await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey,
            cancellationToken: cancellationToken);
        
        _logger.LogDebug(
            "Queue bound: {Queue} -> {Exchange} [{RoutingKey}]",
            queueName,
            exchangeName,
            routingKey);
    }
}
