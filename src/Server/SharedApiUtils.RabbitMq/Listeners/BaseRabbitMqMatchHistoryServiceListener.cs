using System.Text.Json;
using Microsoft.Extensions.Logging;
using SharedApiUtils.RabbitMq.Core.Messages.MatchHistoryService;
using SharedApiUtils.RabbitMq.Core.Messages.RatingService;

namespace SharedApiUtils.RabbitMq.Listeners;

public abstract class BaseRabbitMqMatchHistoryServiceListener : BaseRabbitMqMessageListener
{
    private readonly ILogger<BaseRabbitMqMatchHistoryServiceListener> logger;
    private readonly RabbitMqConnection connection;

    public BaseRabbitMqMatchHistoryServiceListener(
        ILogger<BaseRabbitMqMatchHistoryServiceListener> logger,
        RabbitMqConnection connection,
        ILogger<BaseRabbitMqMessageListener> _logger) : base(connection, _logger)
    {
        this.logger = logger;
        this.connection = connection;
    }
    public async Task StartListening()
    {
        var channel = await connection.GetNewChannel();
        logger.LogDebug($"Using channel {channel.ChannelNumber} for MatchHistoryServiceListener");
        var queue = await channel.QueueDeclareAsync(
            queue: ServicesQueues.MatchHistoryServiceQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        RegisterEventListener(RabbitMqEvents.AddMatchInfo, true, HandleAddMatchInfo);
        await StartBaseListening(false, ServicesQueues.MatchHistoryServiceQueue, channel);
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleAddMatchInfo(byte[] requestBody)
    {
        try
        {
            var request = JsonSerializer.Deserialize<AddMatchInfoRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await AddMatchInfo(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the add match info message");
            return (false, []);
        }
    }
    protected abstract Task<AddMatchInfoReply> AddMatchInfo(AddMatchInfoRequest request);
}
