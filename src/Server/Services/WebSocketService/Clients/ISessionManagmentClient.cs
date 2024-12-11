namespace WebSocketService.Clients;

public interface ISessionManagmentClient
{
    Task AddRoomMember(string member);
    Task RemoveRoomMember(string member);
}
