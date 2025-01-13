namespace SharedApiUtils.RabbitMq.Core.Messages.GameSessionService;

public class StartGameSessionReply
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SessionId { get; set;}
}
