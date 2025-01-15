using System.Threading.Channels;
using RabbitMQ.Client;

namespace SharedApiUtils.RabbitMq;

public class RabbitMqConnection :IDisposable
{
    private readonly RabbitMqConfiguration configuration;
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
        
    }
    public void Dispose()
    {
        
        connection?.Dispose();
    }
    public async Task<IChannel> GetNewChannel()
    {
        var channel = await connection.CreateChannelAsync();
        return channel;
    }
}
