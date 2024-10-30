namespace MatchHistoryService.Models;

public class Score
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int ScorePoints { get; set; }
    public MatchInformation MatchInfo { get; set; }
    public bool IsWinner { get; set; }
}
