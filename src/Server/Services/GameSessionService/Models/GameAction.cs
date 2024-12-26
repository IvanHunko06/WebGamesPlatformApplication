namespace GameSessionService.Models;

public class GameAction
{
    public string PlayerId {  get; set; }
    public string ActionType { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public object Payload {  get; set; }
}
