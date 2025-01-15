using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SharedApiUtils.RabbitMq.Core.Messages.RoomEvents;
using System.Text.Json;

namespace SharedApiUtils.RabbitMq.Listeners;

public abstract class BaseRabbitMqRoomsEventsListener : BaseRabbitMqMessageListener
{
    private readonly RabbitMqConnection connection;
    private readonly ILogger<BaseRabbitMqRoomsEventsListener> logger;

    public BaseRabbitMqRoomsEventsListener(
        RabbitMqConnection connection,
        ILogger<BaseRabbitMqRoomsEventsListener> logger,
        ILogger<BaseRabbitMqMessageListener> _logger1) : base(connection, _logger1)
    {
        this.connection = connection;
        this.logger = logger;
    }
    public async Task StartListening()
    {
        var channel = await connection.GetNewChannel();
        logger.LogDebug($"Using channel {channel.ChannelNumber} for BaseRabbitMqRoomsEventsListener");
        var queue = await channel.QueueDeclareAsync(queue: "", autoDelete: true, exclusive: true, durable: false);
        await channel.ExchangeDeclareAsync(exchange: ServicesExchanges.RoomsEventsExchange,
                type: ExchangeType.Fanout,
                durable: true);
        await channel.QueueBindAsync(queue.QueueName, ServicesExchanges.RoomsEventsExchange, "");

        RegisterEventListener(RabbitMqEvents.OnRoomCreated, false, HandleOnRoomCreated);
        RegisterEventListener(RabbitMqEvents.OnRoomDeleted, false, HandleOnRoomDeleted);
        RegisterEventListener(RabbitMqEvents.OnRoomJoin, false, HandleOnRoomJoin);
        RegisterEventListener(RabbitMqEvents.OnRoomLeave, false, HandleOnRoomLeave);
        await StartBaseListening(true, queue.QueueName, channel);
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleOnRoomCreated(byte[] requestBody)
    {
        try
        {
            OnRoomCreatedEventMessage? request = JsonSerializer.Deserialize<OnRoomCreatedEventMessage>(requestBody);
            if (request is null)
                return (false, []);

            await OnRoomCreated(request);
            return (true, []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while receiving the OnRoomCreated message");
            return (false, []);
        }
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleOnRoomDeleted(byte[] requestBody)
    {
        try
        {
            OnRoomDeletedEventMessage? request = JsonSerializer.Deserialize<OnRoomDeletedEventMessage>(requestBody);
            if (request is null)
                return (false, []);

            await OnRoomDeleted(request);
            return (true, []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while receiving the OnRoomDeleted message");
            return (false, []);
        }
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleOnRoomJoin(byte[] requestBody)
    {
        try
        {
            OnRoomJoinEventMessage? request = JsonSerializer.Deserialize<OnRoomJoinEventMessage>(requestBody);
            if (request is null)
                return (false, []);

            await OnRoomJoin(request);
            return (true, []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while receiving the OnRoomJoin message");
            return (false, []);
        }
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleOnRoomLeave(byte[] requestBody)
    {
        try
        {
            OnRoomLeaveEventMessage? request = JsonSerializer.Deserialize<OnRoomLeaveEventMessage>(requestBody);
            if (request is null)
                return (false, []);

            await OnRoomLeave(request);
            return (true, []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while receiving the OnRoomLeave message");
            return (false, []);
        }
    }

    protected abstract Task OnRoomCreated(OnRoomCreatedEventMessage message);
    protected abstract Task OnRoomDeleted(OnRoomDeletedEventMessage message);
    protected abstract Task OnRoomJoin(OnRoomJoinEventMessage message);
    protected abstract Task OnRoomLeave(OnRoomLeaveEventMessage message);
}
