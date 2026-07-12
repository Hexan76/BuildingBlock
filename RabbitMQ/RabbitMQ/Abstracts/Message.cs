using System;
using System.Collections.Generic;

namespace Framework.RabbitMQ;

public class Message<TPayload>
    where TPayload : class
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Type { get; init; } = default!;

    public Guid? CorrelationId { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public IDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public TPayload Payload { get; init; } = default!;
}
