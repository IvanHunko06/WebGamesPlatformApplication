namespace SharedApiUtils.RabbitMq.Core.Messages.GameProccessingService;

public class CheckGameOverReply
{
    public bool IsOver { get; set; }
    public Dictionary<string, int>? PlayerScores { get; set; }
}
