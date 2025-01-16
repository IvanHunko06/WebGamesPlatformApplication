using System.Text.Json.Serialization;

namespace RoomManagmentService.Models;

public class RoomModel
{
    public string RoomId { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public bool IsPrivate {  get; set; }
    public int SelectedPlayersCount {  get; set; }
    public string Creator { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    [JsonIgnore]
    public int CurrentPlayersCount
    {
        get
        {
            return Members.Count;
        }
    }
    public List<string> Members { get; set; } = [];
    public DateTimeOffset LastModified { get; set; }
}
