using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Framework.RabbitMQ;

public sealed class RabbitConsumerHostedService : IHostedService
{
    private readonly IRabbitConnectionFactory _connectionFactory;
    private readonly ITopologyManager _topologyManager;
    private readonly IConsumePipeline _consumePipeline;
    private readonly ConsumerRegistry _consumerRegistry;
    private readonly IEnumerable<IConsumerRegistrationContributor> _contributors;
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMQOptions _options;
    private readonly ILogger<RabbitConsumerHostedService> _logger;
    private readonly List<(IChannel Channel, string ConsumerTag)> _listeners = [];

    public RabbitConsumerHostedService(
        IRabbitConnectionFactory connectionFactory,
        ITopologyManager topologyManager,
        IConsumePipeline consumePipeline,
        ConsumerRegistry consumerRegistry,
        IEnumerable<IConsumerRegistrationContributor> contributors,
        IServiceProvider serviceProvider,
        IOptions<RabbitMQOptions> options,
        ILogger<RabbitConsumerHostedService> logger)
    {
        _connectionFactory = connectionFactory;
        _topologyManager = topologyManager;
        _consumePipeline = consumePipeline;
        _consumerRegistry = consumerRegistry;
        _contributors = contributors;
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _topologyManager.EnsureManualTopologyAsync(cancellationToken);

        foreach (var contributor in _contributors)
        {
            _consumerRegistry.Add(contributor.CreateRegistration(_options));
        }

        foreach (var registration in _consumerRegistry.Registrations)
        {
            await StartListenerAsync(registration, cancellationToken);
        }

        _logger.LogInformation(
            "Started {Count} RabbitMQ consumer listener(s)",
            _consumerRegistry.Registrations.Count);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var (channel, consumerTag) in _listeners)
        {
            if (channel.IsOpen)
            {
                await channel.BasicCancelAsync(consumerTag, cancellationToken: cancellationToken);
                await channel.CloseAsync(cancellationToken: cancellationToken);
            }

            channel.Dispose();
        }

        _listeners.Clear();
    }

    private async Task StartListenerAsync(ConsumerRegistration registration, CancellationToken cancellationToken)
    {
        if (_options.AutoDeclareExchanges)
        {
            var exchangeType = RabbitMetadataResolver.ResolveExchangeType(registration.MessageType, _options);
            var (durable, autoDelete) = RabbitMetadataResolver.ResolveExchangeFlags(registration.MessageType);

            await _topologyManager.DeclareExchangeAsync(
                registration.ExchangeName,
                exchangeType,
                durable,
                autoDelete,
                cancellationToken);
        }

        if (_options.AutoDeclareQueues)
        {
            await _topologyManager.DeclareQueueAsync(
                registration.QueueName,
                registration.Durable,
                registration.Exclusive,
                registration.AutoDelete,
                cancellationToken);

            await _topologyManager.BindQueueAsync(
                registration.QueueName,
                registration.ExchangeName,
                registration.RoutingKey,
                cancellationToken);
        }

        var channel = await _connectionFactory.CreateChannelAsync(cancellationToken);
        await channel.BasicQosAsync(0, 1, false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            await HandleMessageAsync(registration, channel, eventArgs, cancellationToken);
        };

        var consumerTag = await channel.BasicConsumeAsync(
            queue: registration.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _listeners.Add((channel, consumerTag));

        _logger.LogInformation(
            "Listening on queue {Queue} for {MessageType} via {ConsumerType}",
            registration.QueueName,
            registration.MessageType.Name,
            registration.ConsumerType.Name);
    }

    private async Task HandleMessageAsync(
        ConsumerRegistration registration,
        IChannel channel,
        BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken)
    {
        var rawBody = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

        try
        {
            var payload = JsonSerializer.Deserialize(rawBody, registration.MessageType);
            if (payload == null)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize message to {registration.MessageType.Name}.");
            }

            var headers = eventArgs.BasicProperties.Headers?
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value ?? string.Empty)
                ?? new Dictionary<string, object>();

            // Create Message<TPayload> wrapper instance via reflection
            var wrapperType = typeof(Message<>).MakeGenericType(registration.MessageType);
            var wrapper = Activator.CreateInstance(wrapperType)!;

            // set Payload
            var payloadProp = wrapperType.GetProperty("Payload")!;
            payloadProp.SetValue(wrapper, payload);

            // set Type
            var typeProp = wrapperType.GetProperty("Type")!;
            var msgTypeName = eventArgs.BasicProperties?.Type ?? registration.MessageType.Name;
            typeProp.SetValue(wrapper, msgTypeName);

            // set Id if provided in headers
            if (headers.TryGetValue("x-message-id", out var idObj))
            {
                var idStr = idObj is byte[] b ? Encoding.UTF8.GetString(b) : idObj?.ToString();
                if (Guid.TryParse(idStr, out var id))
                {
                    var idProp = wrapperType.GetProperty("Id")!;
                    idProp.SetValue(wrapper, id);
                }
            }

            // set CorrelationId if present
            if (headers.TryGetValue("x-correlation-id", out var corrObj))
            {
                var corrStr = corrObj is byte[] b ? Encoding.UTF8.GetString(b) : corrObj?.ToString();
                if (Guid.TryParse(corrStr, out var corr))
                {
                    var corrProp = wrapperType.GetProperty("CorrelationId")!;
                    corrProp.SetValue(wrapper, corr);
                }
            }

            // populate wrapper headers (string,string)
            var headersProp = wrapperType.GetProperty("Headers")!;
            var wrapperHeaders = (IDictionary<string, string>?)headersProp.GetValue(wrapper) ?? new Dictionary<string, string>();
            foreach (var header in headers)
            {
                var valueStr = header.Value is byte[] bb ? Encoding.UTF8.GetString(bb) : header.Value?.ToString() ?? string.Empty;
                wrapperHeaders[header.Key] = valueStr;
            }
            headersProp.SetValue(wrapper, wrapperHeaders);

            var context = new ConsumeContext
            {
                MessageType = registration.MessageType,
                Message = wrapper,
                QueueName = registration.QueueName,
                Exchange = registration.ExchangeName,
                RoutingKey = eventArgs.RoutingKey,
                DeliveryTag = eventArgs.DeliveryTag,
                RawBody = rawBody,
                ReplyTo = eventArgs.BasicProperties.ReplyTo,
                CorrelationId = eventArgs.BasicProperties.CorrelationId,
                CancellationToken = cancellationToken
            };

            foreach (var header in headers)
            {
                context.Headers[header.Key] = header.Value;
            }

            await _consumePipeline.ExecuteAsync(
                context,
                () => InvokeConsumerAsync(registration, wrapper, context, cancellationToken),
                cancellationToken);

            if (registration.IsRequestHandler && !string.IsNullOrEmpty(context.ReplyTo))
            {
                await PublishReplyAsync(channel, context, cancellationToken);
            }

            await channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to consume message from queue {Queue}",
                registration.QueueName);

            await channel.BasicNackAsync(eventArgs.DeliveryTag, false, requeue: false, cancellationToken);
        }
    }

    private async Task InvokeConsumerAsync(
        ConsumerRegistration registration,
        object messageWrapper,
        ConsumeContext context,
        CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService(registration.ConsumerType);

        if (registration.IsRequestHandler)
        {
            var handlerInterface = typeof(IMessageRequestHandler<,>)
                .MakeGenericType(registration.MessageType, registration.ResponseType!);
            var handleMethod = handlerInterface.GetMethod("HandleAsync")
                ?? throw new InvalidOperationException(
                    $"Handler {registration.ConsumerType.Name} must implement " +
                    $"IMessageRequestHandler<{registration.MessageType.Name}, {registration.ResponseType!.Name}>");

            var handleTask = handleMethod.Invoke(consumer, new[] { messageWrapper, context, cancellationToken }) as Task
                ?? throw new InvalidOperationException(
                    $"Handler {registration.ConsumerType.Name}.HandleAsync did not return a Task.");

            await handleTask;

            // Task<TResponse>.Result is safe to read once the task has completed.
            context.Response = handleTask.GetType().GetProperty("Result")?.GetValue(handleTask);
            return;
        }

        var consumerInterface = typeof(IMessageConsumer<>).MakeGenericType(registration.MessageType);
        var method = consumerInterface.GetMethod("ConsumeAsync")
            ?? throw new InvalidOperationException(
                $"Consumer {registration.ConsumerType.Name} must implement IMessageConsumer<{registration.MessageType.Name}>");

        var task = method.Invoke(consumer, new[] { messageWrapper, context, cancellationToken }) as Task
            ?? throw new InvalidOperationException(
                $"Consumer {registration.ConsumerType.Name}.ConsumeAsync did not return a Task.");

        await task;
    }

    private async Task PublishReplyAsync(
        IChannel channel,
        ConsumeContext context,
        CancellationToken cancellationToken)
    {
        var replyBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(context.Response));

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            CorrelationId = context.CorrelationId,
            Type = context.Response?.GetType().FullName
        };

        // Direct reply-to: publish to the default exchange using the reply queue as the routing key.
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: context.ReplyTo!,
            mandatory: false,
            basicProperties: properties,
            body: replyBody,
            cancellationToken: cancellationToken);

        _logger.LogDebug(
            "Sent RPC reply for correlationId {CorrelationId} to {ReplyTo}",
            context.CorrelationId,
            context.ReplyTo);
    }
}
