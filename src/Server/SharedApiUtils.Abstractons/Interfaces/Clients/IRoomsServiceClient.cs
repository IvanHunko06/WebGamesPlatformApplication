using SharedApiUtils.Abstractons.Core;

namespace SharedApiUtils.Abstractons.Interfaces.Clients;

public interface IRoomsServiceClient
{
    Task<(RoomModelDto? roomModel, string? errorMessage)> GetRoom(string roomId);
    Task<string?> AddToRoom(string roomId, string userId, string accessToken);
    Task<string?> RemoveFromRoom(string roomId, string userId);
    Task<string?> DeleteRoom(string roomId);
}
