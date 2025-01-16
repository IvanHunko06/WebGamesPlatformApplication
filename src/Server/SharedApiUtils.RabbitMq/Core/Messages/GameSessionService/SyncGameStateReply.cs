namespace SharedApiUtils.RabbitMq.Core.Messages.GameSessionService;

public class SyncGameStateReply
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? GameState { get; set; }
}
