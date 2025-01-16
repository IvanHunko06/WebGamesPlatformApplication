namespace SharedApiUtils.RabbitMq.Core.Messages.GameSessionWsNotifyer;

public class NotifySessionEndedAllUsersRequest
{
    public string SessionId { get; set; }
    public string Reason { get; set; }
    public string? Payload { get; set; }
}
