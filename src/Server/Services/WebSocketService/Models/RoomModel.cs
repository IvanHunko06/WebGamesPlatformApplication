namespace WebSocketService.Models;

public class RoomModel
{
    public string RoomId {  get; set; }
    public string RoomName { get; set; }
    public bool IsPrivate {  get; set; }
    public List<string> Members { get; set; }
    public int SelectedPlayersCount {  get; set; }
    public string Creator {  get; set; }
    public string GameId {  get; set; }
    
}
