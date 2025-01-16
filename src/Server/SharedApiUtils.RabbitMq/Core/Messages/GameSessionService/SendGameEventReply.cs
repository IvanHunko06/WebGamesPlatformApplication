namespace SharedApiUtils.RabbitMq.Core.Messages.GameSessionService;

public class SendGameEventReply
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? GameErrorMessage { get; set; }
}
