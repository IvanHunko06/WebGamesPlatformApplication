namespace SharedApiUtils.Abstractons.Interfaces.Clients;

public interface IMatchHistoryServiceClient
{
    public Task<string?> AddMatchInfo(
        string gameId,
        string finishReason,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        Dictionary<string, int> playerScoresDelta);
}
