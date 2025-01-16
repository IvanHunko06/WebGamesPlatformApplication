using RoomManagmentService.Models;

namespace RoomsService.Interfaces
{
    public interface IRoomsService
    {
        Task<string?> AddUserToRoom(string roomId, string userId, string accessToken);
        Task<(string? errorMessage, string roomId, string accessToken)> CreateRoom(string gameId, string roomName, string creator, bool isPrivate, int selectedPlayersCount);
        Task<string?> DeleteRoom(string roomId, string? compareOwner = null);
        Task<List<RoomModel>> GetPublicRoomsList(string gameId);
        Task<RoomModel?> GetRoom(string roomId);
        Task<List<RoomModel>> GetRoomsList(bool onlyPublic = false, string? gameId = null);
        Task<string?> RemoveUserFromRoom(string userId, string roomId);
    }
}