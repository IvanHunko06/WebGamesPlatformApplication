namespace SharedApiUtils.RabbitMq.Core.Messages.MatchHistoryService;

public class AddMatchInfoRequest
{
    public string GameId { get; set; }
    public string FinishReason { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public Dictionary<string, int> PlayerScoresDelta { get; set; }
}
