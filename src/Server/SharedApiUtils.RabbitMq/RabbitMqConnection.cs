using RabbitMQ.Client;

namespace SharedApiUtils.RabbitMq;

public class RabbitMqConnection :IDisposable
{
    private readonly RabbitMqConfiguration configuration;
    private readonly IChannel channel;
    private readonly IConnection connection;

    public RabbitMqConnection(RabbitMqConfiguration configuration)
    {
        this.configuration = configuration;
        var factory = new ConnectionFactory
        {
            HostName = configuration.Host,
            UserName = configuration.Username,
            Password = configuration.Password
        };
        connection = factory.CreateConnectionAsync().Result;
        channel = connection.CreateChannelAsync().Result;
    }
    public void Dispose()
    {
        channel?.Dispose();
        connection?.Dispose();
    }
    public IChannel GetChannel()
    {
        return channel;
    }
}
