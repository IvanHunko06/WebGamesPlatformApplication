using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SharedApiUtils.RabbitMq.Core.Messages.GameSessionWsNotifyer;

namespace SharedApiUtils.RabbitMq.Listeners;

public abstract class BaseRabbitMqGameSessionWsNotifyerListener : BaseRabbitMqMessageListener
{
    private readonly ILogger<BaseRabbitMqGameSessionWsNotifyerListener> logger;
    private readonly RabbitMqConnection connection;

    public BaseRabbitMqGameSessionWsNotifyerListener(
        ILogger<BaseRabbitMqGameSessionWsNotifyerListener> logger,
        RabbitMqConnection connection, 
        ILogger<BaseRabbitMqMessageListener> _logger ) : base(connection, _logger)
    {
        this.logger = logger;
        this.connection = connection;
    }
    public async Task StartListening()
    {
        var channel = await connection.GetNewChannel();
        logger.LogDebug($"Using channel {channel.ChannelNumber} for game session ws notifyer listener");
        await channel.QueueDeclareAsync(ServicesQueues.GameSessionWsNotifyer, true, false, false);
        RegisterEventListener(RabbitMqEvents.NotifyReciveAction_AllUsers, false, HandleNotifyReciveAction_AllUsers);
        RegisterEventListener(RabbitMqEvents.NotifyReciveAction_User, false, HandleNotifyReciveAction_User);
        RegisterEventListener(RabbitMqEvents.NotifySessionEnded_User, false, HandleNotifySessionEnded_User);
        RegisterEventListener(RabbitMqEvents.NotifySessionEnded_AllUsers, false, HandleNotifySessionEnded_AllUsers);
        await StartBaseListening(true, ServicesQueues.GameSessionWsNotifyer, channel);
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleNotifyReciveAction_AllUsers(byte[] requestBody)
    {
        try
        {
            var request = JsonSerializer.Deserialize<NotifyReciveActionAllUsersRequest>(requestBody);
            if (request is null)
                return (false, []);
            await NotifyReciveAction_AllUsers(request);
            return (true, []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the NotifyReciveAction_AllUsers message");
            return (false, []);
        }
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleNotifyReciveAction_User(byte[] requestBody)
    {
        try
        {
            var request = JsonSerializer.Deserialize<NotifyReciveActionUserRequest>(requestBody);
            if (request is null)
                return (false, []);
            await NotifyReciveAction_User(request);
            return (true, []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the NotifyReciveAction_User message");
            return (false, []);
        }
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleNotifySessionEnded_User(byte[] requestBody)
    {
        try
        {
            var request = JsonSerializer.Deserialize<NotifySessionEndedUserRequest>(requestBody);
            if (request is null)
                return (false, []);
            await NotifySessionEnded_User(request);
            return (true, []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the NotifySessionEnded_User message");
            return (false, []);
        }
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleNotifySessionEnded_AllUsers(byte[] requestBody)
    {
        try
        {
            var request = JsonSerializer.Deserialize<NotifySessionEndedAllUsersRequest>(requestBody);
            if (request is null)
                return (false, []);
            await NotifySessionEnded_AllUsers(request);
            return (true, []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the NotifySessionEnded_AllUsers message");
            return (false, []);
        }
    }
    protected abstract Task NotifyReciveAction_AllUsers(NotifyReciveActionAllUsersRequest request);
    protected abstract Task NotifyReciveAction_User(NotifyReciveActionUserRequest request);
    protected abstract Task NotifySessionEnded_User(NotifySessionEndedUserRequest request);
    protected abstract Task NotifySessionEnded_AllUsers(NotifySessionEndedAllUsersRequest request);
}
