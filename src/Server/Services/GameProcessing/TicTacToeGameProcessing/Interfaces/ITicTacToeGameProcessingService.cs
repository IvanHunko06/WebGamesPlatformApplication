namespace TicTacToeGameProcessing.Interfaces
{
    public interface ITicTacToeGameProcessingService
    {
        string GetEmptySessionState(List<string> players);
        (string? gameErrorMessage, string? newSessionState) UpdateSessionState(string sessionState, string userId, string action, string payload);
        string GetGameStateForPlayer(string sessionState, string userId);
    }
}