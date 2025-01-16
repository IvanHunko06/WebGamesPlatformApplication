using RoomsService.Interfaces;
using SharedApiUtils.Abstractons.Core;
using StackExchange.Redis;
using System.Text.Json;

namespace RoomsService.Repositories;

public class RedisCacheRepository : ICacheRepository
{
    private readonly IDatabase redisDatabase;
    private readonly ILogger<RedisCacheRepository> logger;

    public RedisCacheRepository(RedisHelper redisHelper, ILogger<RedisCacheRepository> logger)
    {
        redisDatabase = redisHelper.GetRedisDatabase();
        this.logger = logger;
    }
    public async Task<GameInfoDto?> GetCachedGameInfo(string gameId)
    {
        try
        {
            string? gameInfoJson = await redisDatabase.StringGetAsync($"Cached:GameInfo:{gameId}");
            if (string.IsNullOrEmpty(gameInfoJson))
                return null;
            GameInfoDto? gameInfo = JsonSerializer.Deserialize<GameInfoDto>(gameInfoJson);
            return gameInfo;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving cached game information.");
            return null;
        }
    }
    public async Task CacheGameInfo(string gameId, GameInfoDto gameInfo)
    {
        try
        {
            string gameInfoJson = JsonSerializer.Serialize(gameInfo);
            await redisDatabase.StringSetAsync($"Cached:GameInfo:{gameId}", gameInfoJson, expiry: TimeSpan.FromHours(1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while caching game information");
        }
    }
    public async Task DeleteCachedGameInfo(string gameId)
    {
        try
        {
            await redisDatabase.KeyDeleteAsync($"Cached:GameInfo:{gameId}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting the game information cache.");
        }
    }
}
