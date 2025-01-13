namespace GameSessionService.Models;

public class GameActionModel
{
    public string PlayerId {  get; set; }
    public string ActionType { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public object Payload {  get; set; }
}
