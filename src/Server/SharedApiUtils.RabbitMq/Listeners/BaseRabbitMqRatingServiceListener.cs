using System.Text.Json;
using Microsoft.Extensions.Logging;
using SharedApiUtils.RabbitMq.Core.Messages.RatingService;
using SharedApiUtils.RabbitMq.Core.Messages.RoomsService;

namespace SharedApiUtils.RabbitMq.Listeners;

public abstract class BaseRabbitMqRatingServiceListener : BaseRabbitMqMessageListener
{
    private readonly ILogger<BaseRabbitMqRatingServiceListener> logger;
    private readonly RabbitMqConnection connection;

    public BaseRabbitMqRatingServiceListener(
        ILogger<BaseRabbitMqRatingServiceListener> logger,
        RabbitMqConnection connection,
        ILogger<BaseRabbitMqMessageListener> _logger) : base(connection, _logger)
    {
        this.logger = logger;
        this.connection = connection;
    }
    public async Task StartListening()
    {
        var channel = await connection.GetNewChannel();
        logger.LogDebug($"Using channel {channel.ChannelNumber} for RatingServiceListener");
        var queue = await channel.QueueDeclareAsync(
            queue: ServicesQueues.RatingServiceQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        RegisterEventListener(RabbitMqEvents.AddLastSeasonUserScore, true, HandleAddLastSeasonUserScore);
        await StartBaseListening(false, ServicesQueues.RatingServiceQueue, channel);
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleAddLastSeasonUserScore(byte[] requestBody)
    {
        try
        {
            var request = JsonSerializer.Deserialize<AddLastSeasonUserScoreRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await AddLastSeasonUserScore(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the add last season user score message");
            return (false, []);
        }
    }
    protected abstract Task<AddLastSeasonUserScoreReply> AddLastSeasonUserScore(AddLastSeasonUserScoreRequest request);
}
