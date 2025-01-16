namespace SharedApiUtils.RabbitMq.Core.Messages.GameSessionService;

public class EndGameSessionReply
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}
