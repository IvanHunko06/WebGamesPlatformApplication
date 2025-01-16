using Google.Protobuf.WellKnownTypes;

namespace WebSocketService;

public class GameIdsList
{
    //private readonly List<string> gameIds;
    //private readonly GamesServiceConnection gamesService;
    //private readonly ILogger<GameIdsList> logger;
    //private readonly RedisHelper redisHelper;

    //public GameIdsList(GamesServiceConnection gamesService, ILogger<GameIdsList> logger, RedisHelper redisHelper)
    //{
    //    gameIds = new List<string>();
    //    this.gamesService = gamesService;
    //    this.logger = logger;
    //    this.redisHelper = redisHelper;
    //}
    public GameIdsList()
    {
        
    }
    public async Task<List<string>> Get()
    {
        //if (gameIds.Count == 0)
        //{
        //    await FillGamesList();
        //}
        //return gameIds;
        return new List<string>();
    }
    //private async Task FillGamesList()
    //{
    //    try
    //    {
    //        logger.LogInformation("Getting gameIds list");
    //        var clientCombination = await gamesService.GetClient();
    //        var gamesList = await clientCombination.client.GetGamesListAsync(new Empty(), clientCombination.headers);
    //        foreach (var game in gamesList.Games)
    //        {
    //            gameIds.Add(game.GameId);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex.ToString());
    //    }

    //}
}
