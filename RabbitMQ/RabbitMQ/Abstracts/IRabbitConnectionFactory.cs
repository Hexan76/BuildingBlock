using RabbitMQ.Client;

namespace Framework.RabbitMQ;

public interface IRabbitConnectionFactory
{
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);

    Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default);
}
