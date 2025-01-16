namespace SharedApiUtils.Abstractons.Interfaces.Clients;


public interface IGameProcessingServiceClient
{
    Task<string> GetEmptySessionState(string gameId, List<string> players);
    Task<(string newSessionState, string? gameErrorMessage)> ProccessAction(string gameId, string sessionState, string userId, string action, string payload);
    Task<string> GetGameStateForPlayer(string gameId, string userId, string sessionState);
    Task<(string? notifyRoomMessage, Dictionary<string, string>? notifyPlayers)> GetSessionDeltaMessages(string gameId, string oldSessionState, string newSessionState);
    Task<(bool IsOver, Dictionary<string, int>? PlayerScores)> CheckGameOver(string gameId, string sessionState);
}