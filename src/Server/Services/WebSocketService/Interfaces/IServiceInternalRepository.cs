namespace WebSocketService.Interfaces;

public interface IServiceInternalRepository
{
    Task SetUserRoom(string userId, string roomId);
    Task<string?> GetUserRoom(string userId);
    Task DeleteUserRoom(string userId);

    Task SetRoomIsStarted(string roomId, bool isStarted);
    Task RemoveRoomIsStarted(string roomId);
    Task<bool?> RoomIsStarted(string roomId);

    Task SetSessionRoom(string sessionId, string roomId);
    Task RemoveSessionRoom(string sessionId);
    Task<string?> GetSessionRoom(string sessionId);
    Task<string?> GetRoomSession(string roomId);
}
