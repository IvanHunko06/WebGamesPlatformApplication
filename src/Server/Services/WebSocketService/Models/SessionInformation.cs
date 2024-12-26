namespace WebSocketService.Models;

public class SessionInformation
{
    public List<string> Players { get; set; }
    public string GameId { get; set; }
    public DateTimeOffset BeginTime { get; set; }
    public DateTimeOffset? EndTime { get; set;}
}
