using WebSocketService.Models;

namespace WebSocketService.Clients;

public interface IRoomsClient
{
    Task AddRoom(RoomClientModel room);
    Task RemoveRoom(string roomId);
    Task UpdateRoom(string roomId, RoomClientModel room);
}
