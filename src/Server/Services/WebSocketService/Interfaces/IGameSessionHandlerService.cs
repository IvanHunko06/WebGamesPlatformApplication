using WebSocketService.Models;

namespace WebSocketService.Interfaces;

public interface IGameSessionHandlerService
{
    Task<SessionInformation> GetSessionInformation(string sessionId, string userId);
    Task<string> SyncGameState(string sessionId, string userId);
    Task<string?> MakeAction(string playerId, string sessionId, string actionName, string payload);
}