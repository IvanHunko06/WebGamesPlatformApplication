using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using RoomManagmentService.Models;
using RoomsService.Protos;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json;

namespace RoomsService.Services;

public class RoomsService : Rooms.RoomsBase
{
    private readonly GamesServiceConnection gamesService;
    private readonly ILogger<RoomsService> logger;
    private readonly IConfiguration configuration;
    private readonly IDatabase redisDatabase;
    private readonly int roomHoursLifetime;
    public RoomsService(GamesServiceConnection gamesService, ILogger<RoomsService> logger, RedisHelper redisHelper, IConfiguration configuration)
    {
        this.gamesService = gamesService;
        this.logger = logger;
        this.configuration = configuration;
        this.redisDatabase = redisHelper.GetRedisDatabase();
        this.roomHoursLifetime = configuration.GetValue<int>("RoomsLifetimeInHours");
    }
    
    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<CreateRoomReply> CreateRoom(CreateRoomRequest request, ServerCallContext context)
    {
        CreateRoomReply reply = new CreateRoomReply();
        try
        {
            string roomId = Guid.NewGuid().ToString();
            string accessToken = "";
            if (request.IsPrivate)
                accessToken = Guid.NewGuid().ToString();


            var clientCombination = await gamesService.GetClient();
            if(clientCombination.client is null || clientCombination.client is null)
            {
                reply.ErrorMessage = ErrorMessages.InternalServerError;
                return reply;
            }

            var gamesListReply = await clientCombination.client.GetGamesListAsync(new Google.Protobuf.WellKnownTypes.Empty(), clientCombination.headers);
            var gameInfo = gamesListReply.Games.Where(g => g.GameId == request.GameId).FirstOrDefault();
            if(gameInfo is null)
            {
                reply.ErrorMessage = ErrorMessages.GameIdNotValid;
                return reply;
            }

            int selectedPlayersCount = 1;
            if (!gameInfo.StaticPlayersCount)
            {
                if (request.SelectedPlayersCount >= gameInfo.MinPlayersCount &&
                    request.SelectedPlayersCount <= gameInfo.MaxPlayersCount)
                {
                    selectedPlayersCount = request.SelectedPlayersCount;
                }
                else
                {
                    reply.ErrorMessage = $"PLAYER_COUNT_MUST_BE_BETWEEN_{gameInfo.MinPlayersCount}_AND_{gameInfo.MaxPlayersCount}";
                    return reply;
                }
            }
            else
            {
                selectedPlayersCount = gameInfo.MaxPlayersCount;
            }

            request.RoomName = request.RoomName.Trim();
            if (request.RoomName.Length < 5)
            {
                reply.ErrorMessage = "ROOM_NAME_LENGHT_REQUIRED_TO_BE_MORE_THAN_FIVE";
                return reply;
            }
            else if (request.RoomName.Length > 20)
            {
                reply.ErrorMessage = "ROOM_NAME_LENGHT_REQUIRED_TO_BE_LESS_THAN_TWENTY";
                return reply;
            }

            var user = context.GetHttpContext().User;
            var userClaims = user.Claims;
            var subjectClaim = userClaims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
            if(subjectClaim is null)
            {
                reply.ErrorMessage = ErrorMessages.SubjectClaimNotFound;
                return reply;
            }

            RoomModel room = new RoomModel()
            {
                IsPrivate = request.IsPrivate,
                RoomId = roomId,
                AccessToken = accessToken,
                GameId = request.GameId,
                SelectedPlayerCount = selectedPlayersCount,
                RoomName = request.RoomName,
                Creator = subjectClaim.Value,    
            };
            await redisDatabase.StringSetAsync(roomId, JsonSerializer.Serialize(room), expiry: TimeSpan.FromHours(roomHoursLifetime));
            await AddRoomIdListItem(roomId);
            reply.IsSuccess = true;
            reply.RoomId = roomId;
            if (request.IsPrivate)
                reply.AccessToken = accessToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }
        
        return reply;
    }
    
    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<DeleteRoomReply> DeleteRoom(DeleteRoomRequest request, ServerCallContext context)
    {
        DeleteRoomReply reply = new DeleteRoomReply();
        try
        {

            if (!(await redisDatabase.KeyExistsAsync(request.RoomId)))
            {
                await DeleteRoomIdListItem(request.RoomId);
                reply.ErrorMessage = ErrorMessages.RoomIdNotExist;
                return reply;
            }
            await redisDatabase.KeyDeleteAsync(request.RoomId);
            await DeleteRoomIdListItem(request.RoomId);
            reply.IsSuccess = true;

        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            reply.ErrorMessage = ErrorMessages.InternalServerError;

        }
        return reply;
    }
    
    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<DeleteRoomReply> DeleteOwnRoom(DeleteRoomRequest request, ServerCallContext context)
    {
        DeleteRoomReply reply = new DeleteRoomReply();
        try
        {
            var room = await GetRoomById(request.RoomId);
            if (room is null)
            {
                reply.ErrorMessage = ErrorMessages.RoomIdNotExist;
                return reply;
            }
            var user = context.GetHttpContext().User;
            var userClaims = user.Claims;
            var subjectClaim = userClaims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
            if (subjectClaim is null)
            {
                reply.ErrorMessage = ErrorMessages.SubjectClaimNotFound;
                return reply;
            }
            if (room.Creator != subjectClaim.Value)
            {
                reply.ErrorMessage = ErrorMessages.NotAllowed;
                return reply;
            }
            await redisDatabase.KeyDeleteAsync(request.RoomId);
            await DeleteRoomIdListItem(request.RoomId);
            reply.IsSuccess = true;
        }
        catch(Exception ex)
        {
            logger.LogError(ex.ToString());
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }
        return reply ;
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<GetRoomsListReply> GetPublicRoomsList(GetPublicRoomsRequest request, ServerCallContext context)
    {
        GetRoomsListRequest getRoomsListRequest = new GetRoomsListRequest()
        {
            GameId = request.GameId,
            OnlyPublicRooms = true
        };
        var reply = await GetRoomsList(getRoomsListRequest, context);
        return reply;

    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<GetRoomsListReply> GetRoomsList(GetRoomsListRequest request, ServerCallContext context)
    {
        GetRoomsListReply reply = new GetRoomsListReply();
        try
        {
            List<string> roomIds = await GetRoomsIdsList();
            foreach (string roomId in roomIds)
            {
                try
                {
                    var roomModel = await GetRoomById(roomId);
                    if (roomModel is null) continue;

                    if (request.HasOnlyPublicRooms && request.OnlyPublicRooms && !roomModel.IsPrivate)
                        continue;

                    if (request.HasGameId && roomModel.GameId != request.GameId)
                        continue;
                    reply.Rooms.Add(new GameRoom()
                    {
                        GameId = roomModel.GameId,
                        IsPrivate = roomModel.IsPrivate,
                        RoomName = roomModel.RoomName,
                        SelectedPlayersCount = roomModel.SelectedPlayerCount,
                        CurrentPlayersCount = roomModel.CurrentPlayersCount,
                        RoomId = roomId,
                        Creator = roomModel.Creator,
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                }
            }
        }catch(Exception ex)
        {
            logger.LogError(ex.ToString());
        }
        

        return reply;
    }

    public override Task<JoinRoomReply> JoinRoom(JoinRoomRequest request, ServerCallContext context)
    {
        return base.JoinRoom(request, context);
    }
    public override Task<LeaveRoomReply> LeaveRoom(LeaveRoomRequest request, ServerCallContext context)
    {
        return base.LeaveRoom(request, context);
    }

    private async Task<List<string>> GetRoomsIdsList()
    {
        List<string> roomIds = new List<string>();
        try
        {
            if (await redisDatabase.KeyExistsAsync("RoomIds"))
            {
                var value = await redisDatabase.StringGetAsync("RoomIds");
                roomIds = JsonSerializer.Deserialize<List<string>>(value) ?? roomIds;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
        }

        return roomIds;
    }
    private async Task AddRoomIdListItem(string roomId)
    {
        List<string> roomsIds = await GetRoomsIdsList();
        try
        {
            roomsIds.Add(roomId);
            await redisDatabase.StringSetAsync("RoomIds", JsonSerializer.Serialize(roomsIds));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
        }

    }
    private async Task DeleteRoomIdListItem(string roomId)
    {
        List<string> roomsIds = await GetRoomsIdsList();
        try
        {
            roomsIds.Remove(roomId);
            await redisDatabase.StringSetAsync("RoomIds", JsonSerializer.Serialize(roomsIds));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
        }
    }
    private async Task<RoomModel?> GetRoomById(string roomId)
    {
        try
        {
            if (!(await redisDatabase.KeyExistsAsync(roomId))) return default;
            string? roomJson = await redisDatabase.StringGetAsync(roomId);
            if (string.IsNullOrEmpty(roomJson)) return default;
            RoomModel? roomModel = JsonSerializer.Deserialize<RoomModel>(roomJson);
            return roomModel;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
