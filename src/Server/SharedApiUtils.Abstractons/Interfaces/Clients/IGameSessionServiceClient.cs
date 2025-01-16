using SharedApiUtils.Abstractons.Core;

namespace SharedApiUtils.Abstractons.Interfaces.Clients;

public interface IGameSessionServiceClient
{
    Task<(string? sessionId, string? errorMessage)> StartGameSession(string roomId);
    Task<(string? errorMessage, GameSessionDto? gameSession)> GetGameSession(string sessionId);
    Task<(string? errorMessage, string? gameErrorMessage)> SendGameEvent(string playerId, string sessionId, string action, string payload);
    Task<string?> EndGameSession(string sessionId, string reason, string? payload);
    Task<(string? errorMessage, string? gameState)> SyncGameState(string playerId, string sessionId);
}
