namespace SharedApiUtils.RabbitMq.Core.Messages.GameProccessingService;

public class GetSessionDeltaMessagesRequest
{
    public string OldSessionState { get; set; }
    public string NewSessionState { get; set; }
}
