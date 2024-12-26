using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using RoomManagmentService.Models;
using RoomsService.Repositories;
using SharedApiUtils;
using SharedApiUtils.Interfaces;
using SharedApiUtils.ServicesAccessing.Protos;
using System.Collections.Concurrent;
using System.Text.Json;
namespace RoomsService.Services;

public class RoomsService : Rooms.RoomsBase
{
    private readonly ILogger<RoomsService> logger;
    private readonly RedisRoomRepository roomRepository;
    private readonly IGamesServiceClient gamesServiceClient;
    private readonly RoomEventNotifier roomEventNotifier;
    private readonly RoomValidationService roomValidationService;
    private readonly UserContextService userContextService;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _roomDeleteTokens = new();
    private static readonly SemaphoreSlim _roomSemaphore = new SemaphoreSlim(1, 1);
    public RoomsService(ILogger<RoomsService> logger,
        RedisRoomRepository roomRepository,
        IGamesServiceClient gamesServiceClient,
        RoomEventNotifier roomEventNotifier,
        RoomValidationService roomValidationService,
        UserContextService userContextService)
    {

        this.logger = logger;
        this.roomRepository = roomRepository;
        this.gamesServiceClient = gamesServiceClient;
        this.roomEventNotifier = roomEventNotifier;
        this.roomValidationService = roomValidationService;
        this.userContextService = userContextService;
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


            var gameInfo = await gamesServiceClient.GetGameInfo(request.GameId);
            if (gameInfo is null)
            {
                reply.ErrorMessage = ErrorMessages.GameIdNotValid;
                return reply;
            }

            int selectedPlayersCount = 1;
            if (!gameInfo.StaticPlayersCount)
            {
                string? validationResult = roomValidationService.ValidatePlayersCount(gameInfo, selectedPlayersCount);
                if (!string.IsNullOrEmpty(validationResult))
                {
                    reply.ErrorMessage = validationResult;
                    return reply;
                }
            }
            else
            {
                selectedPlayersCount = gameInfo.MaxPlayersCount;
            }

            request.RoomName = request.RoomName.Trim();
            string? roomValidationResult = roomValidationService.ValidateRoomName(request.RoomName);
            if (!string.IsNullOrEmpty(roomValidationResult))
            {
                reply.ErrorMessage = roomValidationResult;
                return reply;
            }

            string? userId = userContextService.GetUserId(context);
            if (userId is null)
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
                Creator = userId,
                LastModified = DateTimeOffset.UtcNow
            };
            await roomRepository.AddOrUpdateRoom(room);
            logger.LogInformation($"Room {roomId} has created");
            _ = Task.Run(() => roomEventNotifier.NotifyRoomCreated(room));
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
            var room = await roomRepository.GetRoomById(request.RoomId);
            if (room is null)
            {
                reply.ErrorMessage = ErrorMessages.RoomIdNotExist;
                return reply;
            }

            await roomRepository.DeleteRoom(room.RoomId);
            logger.LogInformation($"Room {room.RoomId} has ben deleted");
            _ = Task.Run(() => roomEventNotifier.NotifyRoomDeleted(room));
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

            var room = await roomRepository.GetRoomById(request.RoomId);
            if (room is null)
            {
                reply.ErrorMessage = ErrorMessages.RoomIdNotExist;
                return reply;
            }
            string? userId = userContextService.GetUserId(context);
            if (userId is null)
            {
                reply.ErrorMessage = ErrorMessages.SubjectClaimNotFound;
                return reply;
            }
            if (room.Creator != userId)
            {
                reply.ErrorMessage = ErrorMessages.NotAllowed;
                return reply;
            }
            await roomRepository.DeleteRoom(request.RoomId);
            _ = Task.Run(() => roomEventNotifier.NotifyRoomDeleted(room));
            logger.LogInformation($"Room {room.RoomId} has ben deleted");
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

            var roomsList = await roomRepository.GetRoomsList();
            foreach (var room in roomsList)
            {
                if (request.HasOnlyPublicRooms && request.OnlyPublicRooms && room.IsPrivate)
                    continue;

                if (request.HasGameId && room.GameId != request.GameId)
                    continue;
                reply.Rooms.Add(new GameRoom()
                {
                    GameId = room.GameId,
                    IsPrivate = room.IsPrivate,
                    RoomName = room.RoomName,
                    SelectedPlayersCount = room.SelectedPlayerCount,
                    CurrentPlayersCount = room.CurrentPlayersCount,
                    RoomId = room.RoomId,
                    Creator = room.Creator,

                });
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
            await _roomSemaphore.WaitAsync();
            try
            {
                var room = await roomRepository.GetRoomById(request.RoomId);
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
                room.LastModified = DateTimeOffset.UtcNow;
                await roomRepository.AddOrUpdateRoom(room);
                _ = Task.Run(() => roomEventNotifier.NotifyRoomJoin(room, request.UserId));
                reply.IsSuccess = true;
            }
            finally
            {
                _roomSemaphore.Release();
            }

        }
        catch (Exception ex)
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

            var room = await roomRepository.GetRoomById(request.RoomId);
            if (room is null)
            {
                reply.ErrorMessage = ErrorMessages.RoomIdNotExist;
                return reply;
            }
            reply.Members.AddRange(room.Members);
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
    public override async Task<RemoveFromRoomReply> RemoveFromRoom(RemoveFromRoomRequest request, ServerCallContext context)
    {
        RemoveFromRoomReply reply = new RemoveFromRoomReply();
        try
        {
            logger.LogInformation($"RemoveFromRoom method;\n Request body: {JsonSerializer.Serialize(request)}");
            await _roomSemaphore.WaitAsync();
            try
            {
                var room = await roomRepository.GetRoomById(request.RoomId);
                if (room is null)
                {
                    reply.ErrorMessage = ErrorMessages.RoomIdNotExist;
                    logger.LogWarning($"RoomId {request.RoomId} not exist");
                    return reply;
                }
                if (!room.Members.Contains(request.UserId))
                {
                    reply.ErrorMessage = ErrorMessages.NotInRoom;
                    logger.LogWarning($"User {request.UserId} not in room {request.RoomId}");
                    return reply;
                }
                room.Members.Remove(request.UserId);
                room.LastModified = DateTimeOffset.UtcNow;
                await roomRepository.AddOrUpdateRoom(room);
                logger.LogInformation($"User {request.UserId} has deleated from room {request.RoomId}");
                _ = Task.Run(() => roomEventNotifier.NotifyRoomLeave(room, request.UserId));
                reply.IsSuccess = true;
            }
            finally
            {
                _roomSemaphore.Release();
            }
            
            
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

            var room = await roomRepository.GetRoomById(request.RoomId);
            if (room is null)
            {
                reply.ErrorMessage = ErrorMessages.RoomIdNotExist;
                return reply;
            }
            GameRoom replyRoom = new GameRoom()
            {
                Creator = room.Creator,
                CurrentPlayersCount = room.CurrentPlayersCount,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomName = room.RoomName,
                RoomId = room.RoomId,
                SelectedPlayersCount = room.SelectedPlayerCount,
            };
            reply.Room = replyRoom;
            reply.Members.AddRange(room.Members);
            reply.IsSuccess = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }

        return reply;
    }




}
