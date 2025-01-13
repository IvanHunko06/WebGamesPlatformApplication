using SharedApiUtils.Abstractons.Core;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.gRPC.ServicesAccessing.Connections;

namespace SharedApiUtils.gRPC.ServicesClients;

public class RPCGamesServiceClient : IGamesServiceClient
{
    private readonly GamesServiceConnection gamesService;

    public RPCGamesServiceClient(GamesServiceConnection gamesService)
    {
        this.gamesService = gamesService;
    }

    public async Task<GameInfoDto?> GetGameInfo(string gameId)
    {
        try
        {
            var clientCombination = await gamesService.GetClient();
            if (clientCombination.client == null) return null;

            var gamesListReply = await clientCombination.client.GetGamesListAsync(new Google.Protobuf.WellKnownTypes.Empty(), clientCombination.headers);
            var game = gamesListReply.Games.FirstOrDefault(g => g.GameId == gameId);
            if (game is null) return null;
            return new GameInfoDto()
            {
                GameId = game.GameId,
                StaticPlayersCount = game.StaticPlayersCount,
                SupportSinglePlayer = game.SupportSinglePlayer,
                MaxPlayersCount = game.MaxPlayersCount,
                MinPlayersCount = game.MinPlayersCount,
            };
        }
        catch (Exception)
        {
            throw;
        }
    }
}
