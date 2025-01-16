using RoomManagmentService.Models;

namespace RoomsService.Interfaces
{
    public interface IRoomRepository
    {
        Task AddOrUpdateRoom(RoomModel room);
        Task DeleteRoom(string roomId);
        Task<RoomModel?> GetRoomById(string roomId);
        Task<List<string>> GetRoomsIdsList();
        Task<List<RoomModel>> GetRoomsList();
    }
}