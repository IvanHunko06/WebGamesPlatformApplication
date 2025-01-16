using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Core.Messages.GameSessionWsNotifyer;
using SharedApiUtils.RabbitMq.Listeners;
using WebSocketService.Interfaces;

namespace WebSocketService.Services;

public class RabbitMqGameSessionWsNotifyer : BaseRabbitMqGameSessionWsNotifyerListener
{
    private readonly IGameSessionWsNotifyer gameSessionWsNotifyer;

    public RabbitMqGameSessionWsNotifyer(
        RabbitMqConnection _connection,
        IGameSessionWsNotifyer gameSessionWsNotifyer,
        ILogger<BaseRabbitMqGameSessionWsNotifyerListener> _logger1, 
        ILogger<BaseRabbitMqMessageListener> _logger2) :base(_logger1, _connection, _logger2)
    {
        this.gameSessionWsNotifyer = gameSessionWsNotifyer;
    }

    protected override async Task NotifySessionEnded_User(NotifySessionEndedUserRequest request)
    {
        await gameSessionWsNotifyer.NotifySessionEnded_User(request.SessionId, request.UserId, request.Reason, request.Payload);
    }

    protected override async Task NotifyReciveAction_AllUsers(NotifyReciveActionAllUsersRequest request)
    {
        await gameSessionWsNotifyer.NotifyReciveAction_Room(request.SessionId, request.Message);
    }

    protected override async Task NotifyReciveAction_User(NotifyReciveActionUserRequest request)
    {
        await gameSessionWsNotifyer.NotifyReciveAction_User(request.SessionId, request.UserId, request.Message);
    }

    protected override async Task NotifySessionEnded_AllUsers(NotifySessionEndedAllUsersRequest request)
    {
        await gameSessionWsNotifyer.NotifySessionEnded_Room(request.SessionId, request.Reason, request.Payload);
    }
}
