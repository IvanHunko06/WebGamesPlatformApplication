namespace SharedApiUtils.RabbitMq.Core.Messages.GameProccessingService;

public class ProccessActionReply
{
    public bool IsSuccess { get; set; }
    public string NewSessionState { get; set; }
    public string? GameErrorMessage { get; set; }
}
