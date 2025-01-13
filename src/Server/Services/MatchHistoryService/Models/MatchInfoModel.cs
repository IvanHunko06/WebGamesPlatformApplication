namespace MatchHistoryService.Models;

public class MatchInfoModel
{
    public DateTimeOffset TimeBegin {  get; set; }
    public DateTimeOffset TimeEnd {  get; set; }
    public string FinishReason {  get; set; }
    public string GameId {  get; set; }
    public Guid RecordId { get; set; }
    public Dictionary<string, int> UserScoreDelta { get; set; }
}
