namespace GameSessionService.Models;

public class GameSessionModel
{
    public string SessionId {  get; set; }
    public string OwnerId {  get; set; }
    public string RoomId { get; set; }
    public string GameId {  get; set; }
    public List<string> Players { get; set; }
    public Dictionary<string, int> PlayerScores { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set;}
    public DateTimeOffset LastUpdated { get; set; }
    public List<GameActionModel> ActionsLog {  get; set; }
    public string SessionState {  get; set; }

}
