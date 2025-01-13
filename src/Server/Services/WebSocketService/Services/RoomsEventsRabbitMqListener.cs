using Microsoft.AspNetCore.SignalR;
using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Core.Messages.RoomEvents;
using SharedApiUtils.RabbitMq.Listeners;
using System.Text.Json;
using WebSocketService.Clients;
using WebSocketService.Hubs;
using WebSocketService.Interfaces;

namespace WebSocketService.Services;

public class RoomsEventsRabbitMqListener : BaseRabbitMqRoomsEventsListener
{
    private readonly ILogger<RoomsEventsRabbitMqListener> logger;
    private readonly IRoomsEventsService roomsEvents;

    public RoomsEventsRabbitMqListener(RabbitMqConnection connection, 
        ILogger<RoomsEventsRabbitMqListener> logger, 
        ILogger<BaseRabbitMqRoomsEventsListener> _logger1,
        ILogger<BaseRabbitMqMessageListener> _logger2,
        IRoomsEventsService roomsEvents
        ) : base(connection, _logger1, _logger2)
    {
        this.logger = logger;
        this.roomsEvents = roomsEvents;
    }
    protected override async Task OnRoomCreated(OnRoomCreatedEventMessage message)
    {
        logger.LogInformation(JsonSerializer.Serialize(message));
    }

    protected override async Task OnRoomDeleted(OnRoomDeletedEventMessage message)
    {
        logger.LogInformation(JsonSerializer.Serialize(message));
    }

    protected override async Task OnRoomJoin(OnRoomJoinEventMessage message)
    {
        logger.LogInformation(JsonSerializer.Serialize(message));
    }

    protected override async Task OnRoomLeave(OnRoomLeaveEventMessage message)
    {
        logger.LogInformation(JsonSerializer.Serialize(message));
    }
}
