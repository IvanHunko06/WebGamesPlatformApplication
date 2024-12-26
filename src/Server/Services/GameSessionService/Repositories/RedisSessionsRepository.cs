using GameSessionService.Interface;
using GameSessionService.Models;
using SharedApiUtils.ServicesAccessing.Protos;
using StackExchange.Redis;
using System.Text.Json;

namespace GameSessionService.Repositories;

public class RedisSessionsRepository : ISessionsRepository
{
    private readonly IDatabase redisDatabase;
    private readonly ILogger<RedisSessionsRepository> logger;

    public RedisSessionsRepository(RedisHelper redisHelper, ILogger<RedisSessionsRepository> logger)
    {
        redisDatabase = redisHelper.GetRedisDatabase();
        this.logger = logger;
    }
    public async Task AddOrUpdateSession(Models.GameSession session)
    {
        try
        {
            await redisDatabase.StringSetAsync(session.SessionId, JsonSerializer.Serialize(session));
            await redisDatabase.SetAddAsync("SessionsIds", session.SessionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error adding or updating session: {session.SessionId}");
            throw;
        }
    }
    public async Task<Models.GameSession?> GetSessionById(string sessionId)
    {
        try
        {
            if (!await redisDatabase.KeyExistsAsync(sessionId)) return null;
            var sessionJson = await redisDatabase.StringGetAsync(sessionId);
            return sessionJson.HasValue ? JsonSerializer.Deserialize<Models.GameSession>(sessionJson) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error retrieving session by ID: {sessionId}");
            return null;
        }
    }

    public async Task DeleteSessionById(string sessionId)
    {
        try
        {
            await redisDatabase.KeyDeleteAsync(sessionId);
            await redisDatabase.SetRemoveAsync("SessionsIds", sessionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error deleting room: {sessionId}");
            throw;
        }
    }
    public async Task<List<string>> GetSessionIdsList()
    {
        try
        {
            var sessionIds = await redisDatabase.SetMembersAsync("SessionsIds");
            return sessionIds.Select(id => id.ToString()).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving session IDs list.");
            return new List<string>();
        }
    }
    public async Task<List<Models.GameSession>> GetSessionsList()
    {
        try
        {
            var roomIds = await redisDatabase.SetMembersAsync("SessionsIds");
            if (roomIds.Length == 0)
            {
                return new List<Models.GameSession>();
            }

            var keys = roomIds.Select(id => (RedisKey)id.ToString()).ToArray();
            var roomValues = await redisDatabase.StringGetAsync(keys);

            var rooms = roomValues
                .Where(value => value.HasValue)
                .Select(value => JsonSerializer.Deserialize<Models.GameSession>(value!))
                .Where(room => room != null)
                .ToList();

            return rooms;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving session list.");
            return new List<Models.GameSession>();
        }
    }

}

