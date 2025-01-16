using GameSessionService.Interfaces;
using SharedApiUtils.Abstractons;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;
using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Core.Messages.RoomEvents;
using SharedApiUtils.RabbitMq.Listeners;

namespace GameSessionService.Services;

public class RabbitMqRoomsEventsListener : BaseRabbitMqRoomsEventsListener
{
    private readonly IGameSessionService gameSessionService;
    private readonly ISessionsRepository sessionsRepository;

    public RabbitMqRoomsEventsListener(
        RabbitMqConnection _connection, 
        ILogger<BaseRabbitMqRoomsEventsListener> _logger1,
        ILogger<BaseRabbitMqMessageListener> _logger2,
        IGameSessionService gameSessionService,
        ISessionsRepository sessionsRepository) : base(_connection, _logger1, _logger2)
    {
        this.gameSessionService = gameSessionService;
        this.sessionsRepository = sessionsRepository;
    }
    protected override Task OnRoomCreated(OnRoomCreatedEventMessage message)
    {
        return Task.CompletedTask;
    }

    protected override async Task OnRoomDeleted(OnRoomDeletedEventMessage message)
    {
        var sessionForRoom = (await sessionsRepository.GetSessionsList()).Where(s => s.RoomId == message.RoomId).FirstOrDefault();
        if (sessionForRoom is null)
            return;

        await gameSessionService.EndGameSession(sessionForRoom.SessionId, EndSessionReason.RoomDeleted, null);
    }

    protected override Task OnRoomJoin(OnRoomJoinEventMessage message)
    {
        return Task.CompletedTask;
    }

    protected override Task OnRoomLeave(OnRoomLeaveEventMessage message)
    {
        return Task.CompletedTask;
    }
}
