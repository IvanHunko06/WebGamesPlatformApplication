namespace SharedApiUtils.RabbitMq.Core.Messages.GameSessionService;

public class EndGameSessionRequest
{
    public string SessionId {  get; set; }
    public string Reason { get; set; }
    public string? Payload { get; set; }
}
