using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RoomManagmentService.Models;
using RoomsService.Interfaces;
using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Core.Messages.RoomEvents;

namespace RoomsService.Services;

public class RabbitmqRoomEventNotifier : IRoomEventNotifier
{
    private readonly RabbitMqConnection connection;
    private readonly ILogger<RabbitmqRoomEventNotifier> logger;
    private static IChannel? channel;

    public RabbitmqRoomEventNotifier(RabbitMqConnection connection, ILogger<RabbitmqRoomEventNotifier> logger)
    {
        this.connection = connection;
        this.logger = logger;
    }
    public async Task NotifyRoomCreated(RoomModel room)
    {
        try
        {
            channel ??= await connection.GetNewChannel();
            await channel.ExchangeDeclareAsync(exchange: ServicesExchanges.RoomsEventsExchange,
                type: ExchangeType.Fanout,
                durable: true);
            BasicProperties properties = new BasicProperties();
            properties.Headers = new Dictionary<string, object?>();
            properties.Headers[WellKnownHeaders.EventType] = RabbitMqEvents.OnRoomCreated;
            properties.Headers[WellKnownHeaders.BodyType] = nameof(OnRoomCreatedEventMessage);
            OnRoomCreatedEventMessage onRoomCreated = new OnRoomCreatedEventMessage()
            {
                Creator = room.Creator,
                CurrentPlayersCount = room.CurrentPlayersCount,
                SelectedPlayersCount = room.SelectedPlayersCount,
                RoomId = room.RoomId,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomName = room.RoomName,
            };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(onRoomCreated));
            await channel.BasicPublishAsync(
                exchange: ServicesExchanges.RoomsEventsExchange,
                routingKey: "",
                body: body,
                basicProperties: properties,
                mandatory: false);

            logger.LogInformation($"event OnRoomCreated sent to {ServicesExchanges.RoomsEventsExchange} exchange");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while notifying the OnRoomCreated event");
        }
    }

    public async Task NotifyRoomDeleted(RoomModel room)
    {
        try
        {
            channel ??= await connection.GetNewChannel();
            await channel.ExchangeDeclareAsync(exchange: ServicesExchanges.RoomsEventsExchange,
                type: ExchangeType.Fanout,
                durable: true);
            BasicProperties properties = new BasicProperties();
            properties.Headers = new Dictionary<string, object?>();
            properties.Headers[WellKnownHeaders.EventType] = RabbitMqEvents.OnRoomDeleted;
            properties.Headers[WellKnownHeaders.BodyType] = nameof(OnRoomDeletedEventMessage);
            OnRoomDeletedEventMessage onRoomDeleted = new OnRoomDeletedEventMessage()
            {
                Creator = room.Creator,
                CurrentPlayersCount = room.CurrentPlayersCount,
                SelectedPlayersCount = room.SelectedPlayersCount,
                RoomId = room.RoomId,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomName = room.RoomName,
                Members = new List<string>(room.Members)
            };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(onRoomDeleted));
            await channel.BasicPublishAsync(
                exchange: ServicesExchanges.RoomsEventsExchange,
                routingKey: "",
                body: body,
                basicProperties: properties,
                mandatory: false);

            logger.LogInformation($"event OnRoomDeleted sent to {ServicesExchanges.RoomsEventsExchange} exchange");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while notifying the OnRoomDeleted event");
        }
    }

    public async Task NotifyRoomJoin(RoomModel room, string joinedMember)
    {
        try
        {
            channel ??= await connection.GetNewChannel();
            await channel.ExchangeDeclareAsync(exchange: ServicesExchanges.RoomsEventsExchange,
                type: ExchangeType.Fanout,
                durable: true);
            BasicProperties properties = new BasicProperties();
            properties.Headers = new Dictionary<string, object?>();
            properties.Headers[WellKnownHeaders.EventType] = RabbitMqEvents.OnRoomJoin;
            properties.Headers[WellKnownHeaders.BodyType] = nameof(OnRoomJoinEventMessage);
            OnRoomJoinEventMessage onRoomJoin = new OnRoomJoinEventMessage()
            {
                Creator = room.Creator,
                CurrentPlayersCount = room.CurrentPlayersCount,
                SelectedPlayersCount = room.SelectedPlayersCount,
                RoomId = room.RoomId,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomName = room.RoomName,
                AddedUserId = joinedMember,
            };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(onRoomJoin));
            await channel.BasicPublishAsync(
                exchange: ServicesExchanges.RoomsEventsExchange,
                routingKey: "",
                body: body,
                basicProperties: properties,
                mandatory: false);

            logger.LogInformation($"event OnRoomJoin sent to {ServicesExchanges.RoomsEventsExchange} exchange");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while notifying the OnRoomJoin event");
        }
    }

    public async Task NotifyRoomLeave(RoomModel room, string removedMember)
    {
        try
        {
            channel ??= await connection.GetNewChannel();
            await channel.ExchangeDeclareAsync(exchange: ServicesExchanges.RoomsEventsExchange,
                type: ExchangeType.Fanout,
                durable: true);
            BasicProperties properties = new BasicProperties();
            properties.Headers = new Dictionary<string, object?>();
            properties.Headers[WellKnownHeaders.EventType] = RabbitMqEvents.OnRoomLeave;
            properties.Headers[WellKnownHeaders.BodyType] = nameof(OnRoomLeaveEventMessage);
            OnRoomLeaveEventMessage onRoomJoin = new OnRoomLeaveEventMessage()
            {
                Creator = room.Creator,
                CurrentPlayersCount = room.CurrentPlayersCount,
                SelectedPlayersCount = room.SelectedPlayersCount,
                RoomId = room.RoomId,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomName = room.RoomName,
                RemovedUserId = removedMember,
            };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(onRoomJoin));
            await channel.BasicPublishAsync(
                exchange: ServicesExchanges.RoomsEventsExchange,
                routingKey: "",
                body: body,
                basicProperties: properties,
                mandatory: false);

            logger.LogInformation($"event OnRoomLeave sent to {ServicesExchanges.RoomsEventsExchange} exchange");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while notifying the OnRoomLeave event");
        }
    }
}
