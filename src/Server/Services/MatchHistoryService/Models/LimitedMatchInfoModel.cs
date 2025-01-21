namespace MatchHistoryService.Models;

public class LimitedMatchInfoModel
{
    public DateTimeOffset TimeBegin { get; set; }
    public DateTimeOffset TimeEnd { get; set; }
    public string FinishReason { get; set; }
    public string GameId { get; set; }
    public int GainedScore { get; set; }
}
