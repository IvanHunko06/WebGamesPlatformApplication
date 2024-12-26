using SharedApiUtils.Interfaces;
using SharedApiUtils.ServicesAccessing.Connections;
using SharedApiUtils.ServicesAccessing.Protos;

namespace SharedApiUtils.ServicesClients;

public class GamesServiceClient : IGamesServiceClient
{
    private readonly GamesServiceConnection gamesService;

    public GamesServiceClient(GamesServiceConnection gamesService)
    {
        this.gamesService = gamesService;
    }

    public async Task<GameInfo?> GetGameInfo(string gameId)
    {
        try
        {
            var clientCombination = await gamesService.GetClient();
            if (clientCombination.client == null) return null;

            var gamesListReply = await clientCombination.client.GetGamesListAsync(new Google.Protobuf.WellKnownTypes.Empty(), clientCombination.headers);
            return gamesListReply.Games.FirstOrDefault(g => g.GameId == gameId);
        }
        catch (Exception)
        {
            throw;
        }
        
    }
}
