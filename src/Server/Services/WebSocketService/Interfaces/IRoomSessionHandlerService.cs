using SharedApiUtils.Abstractons.Core;

namespace WebSocketService.Interfaces;

public interface IRoomSessionHandlerService
{
    Task RemoveFromRoom(string userId, string roomId);
    Task AddToRoom(string userId, string roomId, string accessToken);
    Task<RoomModelDto> GetRoomInformation(string roomId);
    Task StartGame(string roomId, string userId);
}
