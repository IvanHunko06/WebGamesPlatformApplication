namespace MatchHistoryService.Entities;

public class UserScoreEntity
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int ScoreDelta { get; set; }
    public MatchInformationEntity MatchInfo { get; set; }
    public int MatchInfoId {  get; set; }
}
