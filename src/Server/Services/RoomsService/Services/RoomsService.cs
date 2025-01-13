using RoomManagmentService.Models;
using RoomsService.Interfaces;
using RoomsService.Repositories;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Interfaces.Clients;

namespace RoomsService.Services;

public class RoomsService : IRoomsService
{
    private readonly ILogger<RoomsService> logger;
    private readonly IRoomRepository roomRepository;
    private readonly IGamesServiceClient gamesServiceClient;
    private readonly IRoomEventNotifier roomEventNotifier;
    private readonly RoomValidationService roomValidationService;
    private readonly ICacheRepository cacheRepository;
    private static readonly SemaphoreSlim roomSemaphore = new SemaphoreSlim(1, 1);

    public RoomsService(ILogger<RoomsService> logger, IRoomRepository roomRepository,
        IGamesServiceClient gamesServiceClient,
        IRoomEventNotifier roomEventNotifier,
        RoomValidationService roomValidationService,
        ICacheRepository cacheRepository)
    {
        this.logger = logger;
        this.roomRepository = roomRepository;
        this.gamesServiceClient = gamesServiceClient;
        this.roomEventNotifier = roomEventNotifier;
        this.roomValidationService = roomValidationService;
        this.cacheRepository = cacheRepository;
    }
    public async Task<(string? errorMessage, string roomId, string accessToken)> CreateRoom(string gameId, string roomName, string creator, bool isPrivate, int selectedPlayersCount)
    {
        try
        {
            var gameInfo = await cacheRepository.GetCachedGameInfo(gameId);
            if (gameInfo is null)
            {
                logger.LogInformation($"Game {gameId} is not in the cache. Trying to get information about the game.");
                gameInfo = await gamesServiceClient.GetGameInfo(gameId);
                if (gameInfo is null)
                {
                    logger.LogInformation($"Failed to get the game info {gameId} from the games service");
                    return (ErrorMessages.GameIdNotValid, "", "");
                }
                await cacheRepository.CacheGameInfo(gameId, gameInfo);
                logger.LogInformation($"Game info {gameId} successfully cached");
            }

            int playersCount = 1;
            if (!gameInfo.StaticPlayersCount)
            {
                string? validationResult = roomValidationService.ValidatePlayersCount(gameInfo, selectedPlayersCount);
                if (!string.IsNullOrEmpty(validationResult))
                    return (validationResult, "", "");
            }
            else
            {
                playersCount = gameInfo.MaxPlayersCount;
            }

            {
                roomName = roomName.Trim();
                string? validationResult = roomValidationService.ValidateRoomName(roomName);
                if (!string.IsNullOrEmpty(validationResult))
                    return (validationResult, "", "");
            }
            string roomId = Guid.NewGuid().ToString();
            string accessToken = "";
            if (isPrivate)
                accessToken = Guid.NewGuid().ToString();
            RoomModel roomModel = new RoomModel()
            {
                Creator = creator,
                GameId = gameId,
                IsPrivate = isPrivate,
                LastModified = DateTimeOffset.UtcNow,
                RoomName = roomName,
                SelectedPlayersCount = playersCount,
                RoomId = roomId,
                Members = new List<string>(),
                AccessToken = accessToken
            };
            await roomRepository.AddOrUpdateRoom(roomModel);
            logger.LogInformation($"Room {roomModel.RoomId} has created");
            _ = Task.Run(() => roomEventNotifier.NotifyRoomCreated(roomModel));
            return (null, roomId, accessToken);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the room");
            return (ErrorMessages.InternalServerError, "", "");
        }
    }
    public async Task<string?> DeleteRoom(string roomId, string? compareOwner = null)
    {
        try
        {
            var room = await roomRepository.GetRoomById(roomId);
            if (room is null)
                return ErrorMessages.RoomIdNotExist;

            if (!string.IsNullOrEmpty(compareOwner))
            {
                if (room.Creator != compareOwner)
                    return ErrorMessages.NotAllowed;
            }
            await roomRepository.DeleteRoom(room.RoomId);
            logger.LogInformation($"Room {room.RoomId} has ben deleted");
            _ = Task.Run(() => roomEventNotifier.NotifyRoomDeleted(room));
            return null;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while deleting the room {roomId}");
            return ErrorMessages.InternalServerError;

        }
    }
    public async Task<List<RoomModel>> GetRoomsList(bool onlyPublic = false, string? gameId = null)
    {
        try
        {
            var roomsList = await roomRepository.GetRoomsList();
            if (onlyPublic)
                roomsList = roomsList.Where(x => x.IsPrivate == false).ToList();

            if (!string.IsNullOrEmpty(gameId))
                roomsList = roomsList.Where(x => x.GameId == gameId).ToList();

            return roomsList;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while getting the list of rooms with parametrs onlyPublic={onlyPublic}  gameId={gameId ?? "null"}");
            return new List<RoomModel>();
        }
    }
    public async Task<List<RoomModel>> GetPublicRoomsList(string gameId)
    {
        var roomsList = await GetRoomsList(true, gameId);
        return roomsList;
    }

    public async Task<string?> AddUserToRoom(string roomId, string userId, string accessToken)
    {
        try
        {
            await roomSemaphore.WaitAsync();
            try
            {
                logger.LogInformation($"adding user {userId} to room {roomId} with access token {accessToken}");
                var room = await roomRepository.GetRoomById(roomId);
                if (room is null)
                    return ErrorMessages.RoomIdNotExist;

                if (room.Members.Contains(userId))
                    return ErrorMessages.AlreadyInRoom;

                if (room.IsPrivate && room.AccessToken != accessToken)
                    return ErrorMessages.NotAllowed;

                if (room.CurrentPlayersCount + 1 > room.SelectedPlayersCount)
                {
                    return ErrorMessages.RoomIsFull;
                }

                room.Members.Add(userId);
                room.LastModified = DateTimeOffset.UtcNow;
                await roomRepository.AddOrUpdateRoom(room);
                _ = Task.Run(() => roomEventNotifier.NotifyRoomJoin(room, userId)); 
            }
            finally
            {
                roomSemaphore.Release();
            }
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"an error occurred while adding user {userId} to the room {roomId}");
            return ErrorMessages.InternalServerError;
        }
    }
    public async Task<string?> RemoveUserFromRoom(string userId, string roomId)
    {
        try
        {
            await roomSemaphore.WaitAsync();
            try
            {
                logger.LogInformation($"Removing a user {userId} from a room {roomId}");
                var room = await roomRepository.GetRoomById(roomId);
                if (room is null)
                {
                    logger.LogWarning($"RoomId {roomId} not exist");
                    return ErrorMessages.RoomIdNotExist;
                }
                if (!room.Members.Contains(userId))
                {
                    logger.LogWarning($"User {userId} not in room {roomId}");
                    return ErrorMessages.NotInRoom;
                }
                room.Members.Remove(userId);
                room.LastModified = DateTimeOffset.UtcNow;
                await roomRepository.AddOrUpdateRoom(room);
                logger.LogInformation($"User {userId} has removed from room {roomId}");
                _ = Task.Run(() => roomEventNotifier.NotifyRoomLeave(room, userId));     
            }
            finally
            {
                roomSemaphore?.Release();
            }
            return null;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"an error occurred while removing user {userId} from the room {roomId}");
            return ErrorMessages.InternalServerError;
        }
    }

    public async Task<RoomModel?> GetRoom(string roomId)
    {
        try
        {
            var room = await roomRepository.GetRoomById(roomId);
            return room;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while getting the room {roomId}");
            return null;
        }
    }
}
