namespace WebSocketService.Clients;

public interface ISessionManagmentClient
{
    Task AddRoomMember(string member);
    Task RemoveRoomMember(string member);
    Task GameStarted(string sessionId);
    Task ReciveAction(string action);
    Task SessionEnded(string reason, string? payload);
    Task CloseConnection(string reason);
}
