namespace SharedApiUtils.Abstractons.Interfaces.Clients;

public interface IGameSessionWsNotifyerClient
{
    Task NotifyReciveAction_AllUsers(string sessionId, string message);
    Task NotifyReciveAction_User(string sessionId, string userId, string message);
    Task NotifySessionEnded_User(string sessionId, string userId, string endReason, string? payload);
    Task NotifySessionEnded_AllUser(string sessionId, string endReason, string? payload);
}
