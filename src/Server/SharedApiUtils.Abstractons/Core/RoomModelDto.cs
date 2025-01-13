using System.Text.Json.Serialization;

namespace SharedApiUtils.Abstractons.Core;

public class RoomModelDto
{
    public string RoomId { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public int SelectedPlayerCount { get; set; }
    public string Creator { get; set; } = string.Empty;
    public List<string> Members { get; set; } = [];
}
