namespace SharedApiUtils.RabbitMq.Core.Messages.GameSessionWsNotifyer;

public class NotifyReciveActionAllUsersRequest
{
    public string SessionId { get; set; }
    public string Message { get; set; }
}
