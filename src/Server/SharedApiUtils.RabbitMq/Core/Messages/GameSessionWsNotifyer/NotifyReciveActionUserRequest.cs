namespace SharedApiUtils.RabbitMq.Core.Messages.GameSessionWsNotifyer;

public class NotifyReciveActionUserRequest
{
    public string SessionId { get; set; }
    public string UserId { get; set; }
    public string Message { get; set; }
}
