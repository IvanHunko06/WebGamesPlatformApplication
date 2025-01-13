using GameSessionService.Models;

namespace GameSessionService.Interfaces;

public interface IGameSessionService
{
    Task<string?> EndGameSession(string sessionId, string reason, string? payload);
    Task<GameSessionModel?> GetGameSession(string sessionId);
    Task<(string? errorMessage, string? gameErrorMessage)> SendGameEvent(string sessionId, string userId, string action, string payload);
    Task<(string? errorMessage, string? sessionId)> StartGameSession(string roomId);
    Task<(string? errorMessage, string? sessionState)> SyncGameState(string sessionId, string userId);
}