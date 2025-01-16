using StackExchange.Redis;
using WebSocketService.Interfaces;

namespace WebSocketService.Repositories;

public class RedisServiceInternalRepository : IServiceInternalRepository
{
    private readonly IDatabase redisDatabase;

    public RedisServiceInternalRepository(RedisHelper redisHelper)
    {
        redisDatabase = redisHelper.GetRedisDatabase();
    }

    public async Task DeleteUserRoom(string userId)
    {
        try
        {
            await redisDatabase.KeyDeleteAsync($"User{userId}Room");
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<string?> GetUserRoom(string userId)
    {
        try
        {
            string? roomId = await redisDatabase.StringGetAsync($"User{userId}Room");
            return roomId;
        }
        catch (Exception)
        {
            throw;
        }

    }
    public async Task SetUserRoom(string userId, string roomId)
    {
        try
        {
            await redisDatabase.StringSetAsync($"User{userId}Room", roomId);
        }
        catch (Exception)
        {
            throw;
        }

    }

    public async Task SetRoomIsStarted(string roomId, bool isStarted)
    {
        try
        {
            await redisDatabase.StringSetAsync($"Room{roomId}StartStatus", isStarted ? "true" : "false");
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task RemoveRoomIsStarted(string roomId)
    {
        try
        {
            await redisDatabase.KeyDeleteAsync($"Room{roomId}StartStatus");
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<bool?> RoomIsStarted(string roomId)
    {
        try
        {
            string? startStatus = await redisDatabase.StringGetAsync($"Room{roomId}StartStatus");
            if (startStatus is null)
                return null;
            return startStatus == "true" ? true : false;
        }
        catch (Exception)
        {
            throw;
        }

    }

    public async Task SetSessionRoom(string sessionId, string roomId)
    {
        try
        {
            await redisDatabase.StringSetAsync($"Session{sessionId}Room", roomId);
            await redisDatabase.StringSetAsync($"Room{roomId}Session", sessionId);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task RemoveSessionRoom(string sessionId)
    {
        try
        {
            string? roomId = await redisDatabase.StringGetDeleteAsync($"Session{sessionId}Room");
            if (!string.IsNullOrEmpty(roomId))
                await redisDatabase.KeyDeleteAsync($"Room{roomId}Session");
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<string?> GetSessionRoom(string sessionId)
    {
        try
        {
            string? sessionRoom = await redisDatabase.StringGetAsync($"Session{sessionId}Room");
            if (sessionRoom is null)
                return null;
            return sessionRoom;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<string?> GetRoomSession(string roomId)
    {
        try
        {
            string? sessionRoom = await redisDatabase.StringGetAsync($"Room{roomId}Session");
            if (sessionRoom is null)
                return null;
            return sessionRoom;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
