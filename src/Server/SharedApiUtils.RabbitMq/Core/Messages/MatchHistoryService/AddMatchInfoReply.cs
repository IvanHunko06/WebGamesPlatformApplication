namespace SharedApiUtils.RabbitMq.Core.Messages.MatchHistoryService;

public class AddMatchInfoReply
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}
