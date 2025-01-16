namespace SharedApiUtils.Abstractons.Interfaces.Clients;

public interface IRatingServiceClient
{
    Task<string?> AddLastSeasonUserScore(string userId, int addScore);
}
