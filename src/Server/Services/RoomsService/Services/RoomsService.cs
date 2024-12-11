using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using RoomManagmentService.Models;
using RoomsService.Protos;
using SharedApiUtils;
using SharedApiUtils.ServicesAccessing.Connections;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json;
namespace RoomsService.Services;

public class RoomsService : Rooms.RoomsBase
{
    private readonly GamesServiceConnection gamesService;
    private readonly ILogger<RoomsService> logger;
    private readonly IConfiguration configuration;
    private readonly RoomsEventsConnection roomsEvents;
    private readonly IDatabase redisDatabase;
    private readonly int roomHoursLifetime;
    public RoomsService(GamesServiceConnection gamesService, ILogger<RoomsService> logger, RedisHelper redisHelper, IConfiguration configuration, RoomsEventsConnection roomsEvents)
    {
        this.gamesService = gamesService;
        this.logger = logger;
        this.configuration = configuration;
        this.roomsEvents = roomsEvents;
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
            if (clientCombination.client is null || clientCombination.client is null)
            {
                reply.ErrorMessage = ErrorMessages.InternalServerError;
                return reply;
            }

            var gamesListReply = await clientCombination.client.GetGamesListAsync(new Google.Protobuf.WellKnownTypes.Empty(), clientCombination.headers);
            var gameInfo = gamesListReply.Games.Where(g => g.GameId == request.GameId).FirstOrDefault();
            if (gameInfo is null)
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
            if (subjectClaim is null)
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
            if (!room.IsPrivate)
                _ = Task.Run(() => OnRoomCreatedEvent(room));
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


            var room = await GetRoomById(request.RoomId);
            if (room is null)
            {
                reply.ErrorMessage = ErrorMessages.RoomIdNotExist;
                _ = Task.Run(()=>DeleteRoomIdListItem(request.RoomId));
                return reply;
            }
            await redisDatabase.KeyDeleteAsync(request.RoomId);
            await DeleteRoomIdListItem(request.RoomId);
            _ = Task.Run(()=>OnRoomDeleated(room.RoomId,room.GameId));
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
                _ = Task.Run(() => DeleteRoomIdListItem(request.RoomId));
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
            _ = Task.Run(() => OnRoomDeleated(room.RoomId, room.GameId));
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

                    if (request.HasOnlyPublicRooms && request.OnlyPublicRooms && roomModel.IsPrivate)
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
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
        }


        return reply;
    }

    [Authorize(Policy = "OnlyPrivateClient")]
    public override async Task<AddToRoomReply> AddToRoom(AddToRoomRequest request, ServerCallContext context)
    {
        AddToRoomReply reply = new AddToRoomReply();
        try
        {
            
            logger.LogInformation($"AddToRoom method;\n Request body: {JsonSerializer.Serialize(request)}");
            var room = await GetRoomById(request.RoomId);
            if (room is null)
            {
                reply.ErrorMessage = ErrorMessages.RoomIdNotExist;
                return reply;
            }
            if (room.Members.Contains(request.UserId))
            {
                reply.ErrorMessage = ErrorMessages.AlreadyInRoom;
                return reply;
            }
            
            if (room.IsPrivate && room.AccessToken != request.AccessToken)
            {
                reply.ErrorMessage = ErrorMessages.NotAllowed;
                return reply;
            }

            if (room.CurrentPlayersCount + 1 > room.SelectedPlayerCount)
            {
                reply.ErrorMessage = ErrorMessages.RoomIsFull;
                return reply;
            }
            room.Members.Add(request.UserId);
            await redisDatabase.StringSetAsync(request.RoomId, JsonSerializer.Serialize(room), expiry: TimeSpan.FromHours(roomHoursLifetime));
            _ = Task.Run(() => OnRoomJoin(room.RoomId, room.GameId, room.CurrentPlayersCount));
            reply.IsSuccess = true;
        }
        catch(Exception ex)
        {
            logger.LogInformation(ex.ToString());
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }
        return reply;
    }

    [Authorize(Policy = "OnlyPrivateClient")]
    public override async Task<GetRoomMembersReply> GetRoomMembers(GetRoomMembersRequest request, ServerCallContext context)
    {
        GetRoomMembersReply reply = new GetRoomMembersReply();
        try
        {
            var room = await GetRoomById(request.RoomId);
            if (room is null)
            {
                reply.ErrorMessage = ErrorMessages.RoomIdNotExist;
                return reply;
            }
            reply.Members.AddRange(room.Members);
            reply.IsSuccess = true;
        }catch(Exception ex)
        {
            logger.LogInformation(ex.ToString());
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }

        return reply;
    }

    [Authorize(Policy = "OnlyPrivateClient")]
    public override async Task<RemoveFromRoomReply> RemoveFromRoom(RemoveFromRoomRequest request, ServerCallContext context)
    {
        RemoveFromRoomReply reply = new RemoveFromRoomReply();
        try
        {

            logger.LogInformation($"RemoveFromRoom method;\n Request body: {JsonSerializer.Serialize(request)}");
            var room = await GetRoomById(request.RoomId);
            if (room is null)
            {
                reply.ErrorMessage = ErrorMessages.RoomIdNotExist;
                return reply;
            }
            if (!room.Members.Contains(request.UserId))
            {
                reply.ErrorMessage = ErrorMessages.NotInRoom;
                return reply;
            }
            room.Members.Remove(request.UserId);
            await redisDatabase.StringSetAsync(request.RoomId, JsonSerializer.Serialize(room), expiry: TimeSpan.FromHours(roomHoursLifetime));
            _ = Task.Run(()=>OnRoomLeft(room.RoomId, room.GameId, room.CurrentPlayersCount));
            reply.IsSuccess = true;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex.ToString());
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }
        return reply;
    }
    [Authorize(Policy = "OnlyPrivateClient")]
    public override async Task<GetRoomReply> GetRoom(GetRoomRequest request, ServerCallContext context)
    {
        GetRoomReply reply = new GetRoomReply();
        try
        {
            var room = await GetRoomById(request.RoomId);
            if (room is null)
            {
                reply.ErrorMessage = ErrorMessages.RoomIdNotExist;
                return reply;
            }
            GameRoom replyRoon = new GameRoom()
            {
                Creator = room.Creator,
                CurrentPlayersCount = room.CurrentPlayersCount,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomName = room.RoomName,
                RoomId = room.RoomId,
                SelectedPlayersCount = room.SelectedPlayerCount
            };
            reply.Room = replyRoon;
            reply.Members.AddRange(room.Members);
            reply.IsSuccess = true;
        }catch(Exception ex)
        {
            logger.LogError(ex.ToString());
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }

        return reply;
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

    private async Task OnRoomCreatedEvent(RoomModel room)
    {
        try
        {
            var clientCombination = await roomsEvents.GetClient();
            var request = new SharedApiUtils.ServicesAccessing.Protos.OnRoomCreatedRequest()
            {
                GameId = room.GameId,
                RoomId = room.RoomId,
                RoomName = room.RoomName,
                SelectedPlayerCount = room.SelectedPlayerCount,
                Creator = room.Creator,
            };
            await clientCombination.client.OnRoomCreatedAsync(request, clientCombination.headers);
        }
        catch(Exception ex)
        {
            logger.LogError(ex.ToString());
        }

    }
    private async Task OnRoomDeleated(string roomId, string gameId)
    {
        try
        {
            var clientCombination = await roomsEvents.GetClient();
            var request = new SharedApiUtils.ServicesAccessing.Protos.OnRoomDeleatedRequest()
            {
                RoomId = roomId,
                GameId = gameId,
            };
            var reply = await clientCombination.client.OnRoomDeleatedAsync(request, clientCombination.headers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
        }
    }

    private async Task OnRoomJoin(string roomId, string gameId, int membersCount)
    {
        try
        {
            var clientCombination = await roomsEvents.GetClient();
            var request = new SharedApiUtils.ServicesAccessing.Protos.OnRoomJoinRequest()
            {
                RoomId = roomId,
                GameId = gameId,
                MembersCount = membersCount
            };
            var reply = await clientCombination.client.OnRoomJoinAsync(request, clientCombination.headers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
        }
    }
    private async Task OnRoomLeft(string roomId, string gameId, int membersCount)
    {
        try
        {
            var clientCombination = await roomsEvents.GetClient();
            var request = new SharedApiUtils.ServicesAccessing.Protos.OnRoomLeaveRequest()
            {
                RoomId = roomId,
                GameId = gameId,
                MembersCount = membersCount
            };
            var reply = await clientCombination.client.OnRoomLeaveAsync(request, clientCombination.headers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
        }
    }
}
