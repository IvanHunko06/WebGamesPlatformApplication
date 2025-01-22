namespace MatchHistoryService.Models;

public class FullMatchInfoModel
{
    public DateTimeOffset TimeBegin { get; set; }
    public DateTimeOffset TimeEnd { get; set; }
    public string FinishReason { get; set; }
    public string GameId { get; set; }
    public Guid RecordId { get; set; }
    public int GainedScore { get; set; }
    public Dictionary<string, int> UserScoreDelta { get; set; }
}
