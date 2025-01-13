using RoomManagmentService.Models;
using RoomsService.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace RoomsService.Repositories;

public class RedisRoomRepository : IRoomRepository
{
    private readonly ILogger<RedisRoomRepository> logger;
    private readonly IDatabase redisDatabase;
    public RedisRoomRepository(RedisHelper redisHelper, ILogger<RedisRoomRepository> logger)
    {
        this.logger = logger;
        this.redisDatabase = redisHelper.GetRedisDatabase();

    }
    public async Task<RoomModel?> GetRoomById(string roomId)
    {
        try
        {
            if (!await redisDatabase.KeyExistsAsync(roomId)) return null;
            var roomJson = await redisDatabase.StringGetAsync(roomId);
            return roomJson.HasValue ? JsonSerializer.Deserialize<RoomModel>(roomJson) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving room by ID: {RoomId}", roomId);
            return null;
        }
    }
    public async Task<List<string>> GetRoomsIdsList()
    {
        try
        {
            var roomIds = await redisDatabase.SetMembersAsync("RoomIds");
            return roomIds.Select(id => id.ToString()).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving room IDs list.");
            return new List<string>();
        }
    }

    public async Task AddOrUpdateRoom(RoomModel room)
    {
        try
        {
            await redisDatabase.StringSetAsync(room.RoomId, JsonSerializer.Serialize(room));
            await redisDatabase.SetAddAsync("RoomIds", room.RoomId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding or updating room: {RoomId}", room.RoomId);
            throw;
        }

    }
    public async Task DeleteRoom(string roomId)
    {
        try
        {
            await redisDatabase.KeyDeleteAsync(roomId);
            await redisDatabase.SetRemoveAsync("RoomIds", roomId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting room: {RoomId}", roomId);
            throw;
        }
    }
    public async Task<List<RoomModel>> GetRoomsList()
    {
        try
        {
            var roomIds = await redisDatabase.SetMembersAsync("RoomIds");
            if (roomIds.Length == 0)
            {
                return new List<RoomModel>();
            }

            var keys = roomIds.Select(id => (RedisKey)id.ToString()).ToArray();
            var roomValues = await redisDatabase.StringGetAsync(keys);

            var rooms = roomValues
                .Where(value => value.HasValue)
                .Select(value => JsonSerializer.Deserialize<RoomModel>(value!))
                .Where(room => room != null)
                .ToList();

            return rooms;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving room list.");
            return new List<RoomModel>();
        }
    }

}
