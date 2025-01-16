using System.Text.Json;
using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Core.Messages.RoomEvents;
using SharedApiUtils.RabbitMq.Listeners;
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
        await roomsEvents.InvokedOnRoomCreated(new Models.RoomModel()
        {
            Creator = message.Creator,
            SelectedPlayersCount = message.SelectedPlayersCount,
            GameId = message.GameId,
            IsPrivate = message.IsPrivate,
            Members = new List<string>(),
            RoomId = message.RoomId,
            RoomName = message.RoomName,
        });
    }

    protected override async Task OnRoomDeleted(OnRoomDeletedEventMessage message)
    {
        await roomsEvents.InvokedOnRoomDeleted(new Models.RoomModel()
        {
            Creator = message.Creator,
            SelectedPlayersCount = message.SelectedPlayersCount,
            GameId = message.GameId,
            IsPrivate = message.IsPrivate,
            Members = new List<string>(),
            RoomId = message.RoomId,
            RoomName = message.RoomName,
        });
    }

    protected override async Task OnRoomJoin(OnRoomJoinEventMessage message)
    {
        await roomsEvents.InvokedOnRoomJoin(new Models.RoomModel()
        {
            Creator = message.Creator,
            SelectedPlayersCount = message.SelectedPlayersCount,
            GameId = message.GameId,
            IsPrivate = message.IsPrivate,
            Members = new List<string>(),
            RoomId = message.RoomId,
            RoomName = message.RoomName,
        }, message.AddedUserId);
    }

    protected override async Task OnRoomLeave(OnRoomLeaveEventMessage message)
    {
        await roomsEvents.InvokedOnRoomLeave(new Models.RoomModel()
        {
            Creator = message.Creator,
            SelectedPlayersCount = message.SelectedPlayersCount,
            GameId = message.GameId,
            IsPrivate = message.IsPrivate,
            Members = new List<string>(),
            RoomId = message.RoomId,
            RoomName = message.RoomName,
        }, message.RemovedUserId);
    }
}
