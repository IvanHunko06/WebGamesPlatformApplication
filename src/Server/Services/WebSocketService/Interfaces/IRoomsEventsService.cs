using WebSocketService.Models;

namespace WebSocketService.Interfaces
{
    public interface IRoomsEventsService
    {
        Task InvokedOnRoomCreated(RoomModel room);
        Task InvokedOnRoomDeleted(RoomModel room);
        Task InvokedOnRoomJoin(RoomModel room, string joinedMember);
        Task InvokedOnRoomLeave(RoomModel room, string deletedMember);
    }
}