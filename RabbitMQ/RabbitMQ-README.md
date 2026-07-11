# Framework.RabbitMQ - Usage Examples

## Registering services

In `Program.cs` or `Startup.cs`:

```csharp
services.AddFrameworkRabbitMQ(options =>
{
    // configure options from config
});

// scan and register consumers from assemblies
services.AddRabbitMQConsumers(typeof(OrderCreatedConsumer).Assembly);
```

## Example consumer that publishes another message

```csharp
[RabbitQueue("orders.queue")]
public class OrderCreatedConsumer : IMessageConsumer<OrderCreatedDto>
{
    private readonly IMessagePublisher _publisher;

    public OrderCreatedConsumer(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task ConsumeAsync(Message<OrderCreatedDto> message, ConsumeContext context, CancellationToken cancellationToken = default)
    {
        var order = message.Payload;

        var outMsg = new Message<InventoryUpdateDto>
        {
            Payload = new InventoryUpdateDto { ProductId = order.ProductId, Quantity = order.Quantity },
            Type = "InventoryUpdate",
            CorrelationId = message.Id
        };

        await _publisher.PublishAsync(outMsg, cancellationToken: cancellationToken);
    }
}
```

## Notes
- Use `[RabbitQueue("name")]` on consumer classes to override the queue name.
- Messages use a stable `Message<TPayload>` envelope with metadata and headers.
- `IMessagePublisher.PublishAsync` accepts a `Message<TPayload>` and copies message metadata into AMQP headers.
