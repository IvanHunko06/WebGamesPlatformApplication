using SharedApiUtils.Abstractons.Core;

namespace SharedApiUtils.Abstractons.Interfaces.Clients;

public interface IGamesServiceClient
{
    Task<GameInfoDto?> GetGameInfo(string gameId);
}
