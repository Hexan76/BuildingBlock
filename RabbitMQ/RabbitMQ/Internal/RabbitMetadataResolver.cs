namespace Framework.RabbitMQ;

internal static class RabbitMetadataResolver
{
    public static string ResolveQueueName(Type consumerType, Type messageType)
    {
        var queueAttribute = consumerType.GetCustomAttributes(typeof(RabbitQueueAttribute), false)
            .Cast<RabbitQueueAttribute>()
            .FirstOrDefault();

        if (queueAttribute != null)
        {
            return queueAttribute.QueueName;
        }

        return $"{messageType.Name.ToKebabCase()}-queue";
    }

    public static (bool Durable, bool Exclusive, bool AutoDelete) ResolveQueueFlags(Type consumerType)
    {
        var queueAttribute = consumerType.GetCustomAttributes(typeof(RabbitQueueAttribute), false)
            .Cast<RabbitQueueAttribute>()
            .FirstOrDefault();

        return queueAttribute == null
            ? (true, false, false)
            : (queueAttribute.Durable, queueAttribute.Exclusive, queueAttribute.AutoDelete);
    }

    public static string ResolveExchangeName(Type type, RabbitMQOptions options)
    {
        var exchangeAttribute = type.GetCustomAttributes(typeof(RabbitExchangeAttribute), true)
            .Cast<RabbitExchangeAttribute>()
            .FirstOrDefault();

        return exchangeAttribute?.ExchangeName ?? options.DefaultExchange;
    }

    public static string ResolveExchangeType(Type type, RabbitMQOptions options)
    {
        var exchangeAttribute = type.GetCustomAttributes(typeof(RabbitExchangeAttribute), true)
            .Cast<RabbitExchangeAttribute>()
            .FirstOrDefault();

        return exchangeAttribute?.ExchangeType ?? options.DefaultExchangeType;
    }

    public static (bool Durable, bool AutoDelete) ResolveExchangeFlags(Type type)
    {
        var exchangeAttribute = type.GetCustomAttributes(typeof(RabbitExchangeAttribute), true)
            .Cast<RabbitExchangeAttribute>()
            .FirstOrDefault();

        return exchangeAttribute == null
            ? (true, false)
            : (exchangeAttribute.Durable, exchangeAttribute.AutoDelete);
    }

    public static string ResolveRoutingKey(Type type)
    {
        var routingKeyAttribute = type.GetCustomAttributes(typeof(RabbitRoutingKeyAttribute), true)
            .Cast<RabbitRoutingKeyAttribute>()
            .FirstOrDefault();

        if (routingKeyAttribute != null)
        {
            return routingKeyAttribute.RoutingKey;
        }

        return type.Name.ToKebabCase();
    }

    private static string ToKebabCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return string.Concat(
                value.Select((character, index) =>
                    char.IsUpper(character) && index > 0
                        ? $"-{char.ToLowerInvariant(character)}"
                        : char.ToLowerInvariant(character).ToString()))
            .Trim('-');
    }
}
