namespace SharedApiUtils.RabbitMq.Core.Messages.GameSessionWsNotifyer;

public class NotifySessionEndedUserRequest
{
    public string SessionId { get; set; }
    public string UserId { get; set; }
    public string Reason { get; set; }
    public string? Payload { get; set; }
}
