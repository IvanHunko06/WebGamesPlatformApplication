namespace MatchHistoryService.Entities;

public class MatchInformationEntity
{
    public int Id { get; set; }
    public DateTimeOffset TimeBegin { get; set; }
    public DateTimeOffset TimeEnd { get; set; }
    public string FinishReason { get; set; }
    public string GameId { get; set; }
    public Guid RecordId { get; set; }
    public List<UserScoreEntity> UserScores { get; set; }
}
