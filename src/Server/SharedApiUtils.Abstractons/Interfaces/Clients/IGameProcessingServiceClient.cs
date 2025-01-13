namespace SharedApiUtils.Abstractons.Interfaces.Clients;


public interface IGameProcessingServiceClient
{
    Task<string> GetEmptySessionState(string gameId, IEnumerable<string> players);
    Task<(string newSessionState, string? gameErrorMessage)> ProccessAction(string gameId, string sessionState, string userId, string action, string payload);
    Task<string> GetGameStateForPlayer(string gameId, string userId, string sessionState);
}