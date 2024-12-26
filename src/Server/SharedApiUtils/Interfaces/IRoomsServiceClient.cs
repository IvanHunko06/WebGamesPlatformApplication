using SharedApiUtils.ServicesAccessing.Protos;

namespace SharedApiUtils.Interfaces;

public interface IRoomsServiceClient
{
    Task<GetRoomReply?> GetRoom(string roomId);
    Task<AddToRoomReply?> AddToRoom(string roomId, string userId, string accessToken);
    Task<RemoveFromRoomReply?> RemoveFromRoom(string roomId, string userId);
    Task<DeleteRoomReply?> DeleteRoom(string roomId);
}
