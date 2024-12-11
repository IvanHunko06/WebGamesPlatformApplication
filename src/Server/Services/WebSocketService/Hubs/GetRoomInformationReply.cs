using WebSocketService.Models;

namespace WebSocketService.Hubs;

public class GetRoomInformationReply
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public RoomModel? Room { get; set; } = null;
    public List<string>? Members { get; set; } = null;
}
