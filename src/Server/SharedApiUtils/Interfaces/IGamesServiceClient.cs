using SharedApiUtils.ServicesAccessing.Protos;

namespace SharedApiUtils.Interfaces;

public interface IGamesServiceClient
{
    Task<GameInfo?> GetGameInfo(string gameId);
}
