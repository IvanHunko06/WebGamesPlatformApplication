using WebSocketService.Models;

namespace WebSocketService.Clients;

public interface IRoomsClient
{
    Task AddRoom(RoomModel room);
    Task RemoveRoom(string roomId);
    Task UpdateRoom(string roomId, RoomModel room);
}
