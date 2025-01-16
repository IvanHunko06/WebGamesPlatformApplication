namespace WebSocketService.Interfaces;

public interface IGameSessionWsNotifyer
{
    Task NotifyReciveAction_Room(string sessionId, string message);
    Task NotifyReciveAction_User(string sessionId, string userId, string message);
    Task NotifySessionEnded_User(string sessionId, string userId, string reason, string? payload);
    Task NotifySessionEnded_Room(string sessionId, string reason, string? payload);
}