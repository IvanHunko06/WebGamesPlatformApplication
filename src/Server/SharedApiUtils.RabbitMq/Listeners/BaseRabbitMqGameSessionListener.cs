using Microsoft.Extensions.Logging;
using SharedApiUtils.RabbitMq.Core.Messages.GameSessionService;
using System.Text.Json;

namespace SharedApiUtils.RabbitMq.Listeners;

public abstract class BaseRabbitMqGameSessionListener : BaseRabbitMqMessageListener
{
    private readonly RabbitMqConnection connection;
    private readonly ILogger<BaseRabbitMqGameSessionListener> logger;

    public BaseRabbitMqGameSessionListener(
        RabbitMqConnection connection,
        ILogger<BaseRabbitMqGameSessionListener> logger,
        ILogger<BaseRabbitMqMessageListener> _logger) : base(connection, _logger)
    {
        this.connection = connection;
        this.logger = logger;

    }
    public async Task StartListening()
    {
        var channel = await connection.GetNewChannel();
        logger.LogDebug($"Using channel {channel.ChannelNumber} for GameSessionListener");
        var queue = await channel.QueueDeclareAsync(
            queue: ServicesQueues.GameSessionServiceQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        RegisterEventListener(RabbitMqEvents.StartGameSession, true, HandleStartGameSession);
        RegisterEventListener(RabbitMqEvents.EndGameSession, true, HandleEndGameSession);
        RegisterEventListener(RabbitMqEvents.GetGameSession, true, HandleGetGameSession);
        RegisterEventListener(RabbitMqEvents.SendGameEvent, true, HandleSendGameEvent);
        RegisterEventListener(RabbitMqEvents.SyncGameState, true, HandleSyncGameState);
        await StartBaseListening(false, ServicesQueues.GameSessionServiceQueue, channel);
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleStartGameSession(byte[] requestBody)
    {
        try
        {
            StartGameSessionRequest? request = JsonSerializer.Deserialize<StartGameSessionRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await StartGameSession(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the start game session message");
            return (false, []);
        }
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleEndGameSession(byte[] requestBody)
    {
        try
        {
            EndGameSessionRequest? request = JsonSerializer.Deserialize<EndGameSessionRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await EndGameSession(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the end game session message");
            return (false, []);
        }
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleGetGameSession(byte[] requestBody)
    {
        try
        {
            GetGameSessionRequest? request = JsonSerializer.Deserialize<GetGameSessionRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await GetGameSession(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the end game session message");
            return (false, []);
        }
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleSendGameEvent(byte[] requestBody)
    {
        try
        {
            SendGameEventRequest? request = JsonSerializer.Deserialize<SendGameEventRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await SendGameEvent(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the end game session message");
            return (false, []);
        }
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleSyncGameState(byte[] requestBody)
    {
        try
        {
            SyncGameStateRequest? request = JsonSerializer.Deserialize<SyncGameStateRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await SyncGameState(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the sync game state message");
            return (false, []);
        }
    }

    protected abstract Task<StartGameSessionReply> StartGameSession(StartGameSessionRequest request);
    protected abstract Task<EndGameSessionReply> EndGameSession(EndGameSessionRequest request);
    protected abstract Task<GetGameSessionReply> GetGameSession(GetGameSessionRequest request);
    protected abstract Task<SendGameEventReply> SendGameEvent(SendGameEventRequest request);
    protected abstract Task<SyncGameStateReply> SyncGameState(SyncGameStateRequest request);
}
