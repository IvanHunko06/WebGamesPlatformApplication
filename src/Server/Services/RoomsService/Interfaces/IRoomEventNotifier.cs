using RoomManagmentService.Models;

namespace RoomsService.Interfaces
{
    public interface IRoomEventNotifier
    {
        Task NotifyRoomCreated(RoomModel room);
        Task NotifyRoomDeleted(RoomModel room);
        Task NotifyRoomJoin(RoomModel room, string joinedMember);
        Task NotifyRoomLeave(RoomModel room, string removedMember);
    }
}