namespace WebSocketService.Clients;

public interface ISessionManagmentClient
{
    Task AddRoomMember(string member);
    Task RemoveRoomMember(string member);
    Task GameStarted(string sessionId);
    Task ReciveAction(string action);
    Task SessionEnded(string sessionId);
    Task UserReconnects(string userId);
    Task UserReconnected(string userId);
    Task CloseConnection(string reason);
}
