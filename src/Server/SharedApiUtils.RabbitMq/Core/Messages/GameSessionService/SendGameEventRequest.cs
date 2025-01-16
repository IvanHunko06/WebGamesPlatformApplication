namespace SharedApiUtils.RabbitMq.Core.Messages.GameSessionService;

public class SendGameEventRequest
{
    public string PlayerId { get; set; }
    public string SessionId { get; set; }
    public string Action { get; set; }
    public string Payload { get; set; }
}
