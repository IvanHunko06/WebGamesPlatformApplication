using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using RoomsService.Interfaces;
using SharedApiUtils.Abstractons;
using SharedApiUtils.gRPC;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;
namespace RoomsService.Services;

public class RoomsRpcService : Rooms.RoomsBase
{


    private readonly IRoomsService roomsService;
    private readonly UserContextService userContextService;

    public RoomsRpcService(
        IRoomsService roomsService,
        UserContextService userContextService)
    {
        this.roomsService = roomsService;
        this.userContextService = userContextService;
    }

    [Authorize(Policy = "OnlyPublicClient")]
    public override async Task<CreateRoomReply> CreateRoom(CreateRoomRequest request, ServerCallContext context)
    {
        CreateRoomReply reply = new CreateRoomReply();
        string? userId = userContextService.GetUserId(context.GetHttpContext());
        if (userId is null)
        {
            reply.ErrorMessage = ErrorMessages.PreferedUsernameClaimNotFound;
            return reply;
        }
        var createResult = await roomsService.CreateRoom(request.GameId, request.RoomName, userId, request.IsPrivate, request.SelectedPlayersCount);
        if (!string.IsNullOrEmpty(createResult.errorMessage))
        {
            reply.ErrorMessage = createResult.errorMessage;
            return reply;
        }
        reply.RoomId = createResult.roomId;
        reply.IsSuccess = true;
        if (!string.IsNullOrEmpty(createResult.accessToken))
            reply.AccessToken = createResult.accessToken;

        return reply;
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<DeleteRoomReply> DeleteRoom(DeleteRoomRequest request, ServerCallContext context)
    {
        DeleteRoomReply reply = new DeleteRoomReply();
        string? errorMessage = await roomsService.DeleteRoom(request.RoomId);
        if (string.IsNullOrEmpty(errorMessage))
            reply.IsSuccess = true;
        else
            reply.ErrorMessage = errorMessage;

        return reply;
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<DeleteRoomReply> DeleteOwnRoom(DeleteRoomRequest request, ServerCallContext context)
    {
        DeleteRoomReply reply = new DeleteRoomReply();
        string? userId = userContextService.GetUserId(context.GetHttpContext());
        if (userId is null)
        {
            reply.ErrorMessage = ErrorMessages.PreferedUsernameClaimNotFound;
            return reply;
        }
        string? errorMessage = await roomsService.DeleteRoom(request.RoomId, userId);
        if (string.IsNullOrEmpty(errorMessage))
            reply.IsSuccess = true;
        else
            reply.ErrorMessage = errorMessage;
        return reply;
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<GetRoomsListReply> GetPublicRoomsList(GetPublicRoomsRequest request, ServerCallContext context)
    {
        GetRoomsListReply reply = new GetRoomsListReply();
        var rooms = await roomsService.GetRoomsList(true, request.GameId);
        reply.Rooms.AddRange(rooms.Select(x => new GameRoom()
        {
            Creator = x.Creator,
            CurrentPlayersCount = x.CurrentPlayersCount,
            SelectedPlayersCount = x.SelectedPlayersCount,
            GameId = x.GameId,
            IsPrivate = x.IsPrivate,
            RoomId = x.RoomId,
            RoomName = x.RoomName,
        }).ToList());
        return reply;

    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<GetRoomsListReply> GetRoomsList(GetRoomsListRequest request, ServerCallContext context)
    {
        GetRoomsListReply reply = new GetRoomsListReply();
        var rooms = await roomsService.GetRoomsList(request.OnlyPublicRooms, request.GameId);
        reply.Rooms.AddRange(rooms.Select(x => new GameRoom()
        {
            Creator = x.Creator,
            CurrentPlayersCount = x.CurrentPlayersCount,
            SelectedPlayersCount = x.SelectedPlayersCount,
            GameId = x.GameId,
            IsPrivate = x.IsPrivate,
            RoomId = x.RoomId,
            RoomName = x.RoomName,
        }).ToList());
        return reply;
    }

    [Authorize(Policy = "OnlyPrivateClient")]
    public override async Task<AddToRoomReply> AddToRoom(AddToRoomRequest request, ServerCallContext context)
    {
        AddToRoomReply reply = new AddToRoomReply();
        string? errorMessage = await roomsService.AddUserToRoom(request.RoomId, request.UserId, request.AccessToken);
        if (string.IsNullOrEmpty(errorMessage))
            reply.IsSuccess = true;
        else
            reply.ErrorMessage = errorMessage;
        return reply;
    }


    [Authorize(Policy = "OnlyPrivateClient")]
    public override async Task<RemoveFromRoomReply> RemoveFromRoom(RemoveFromRoomRequest request, ServerCallContext context)
    {
        RemoveFromRoomReply reply = new RemoveFromRoomReply();
        string? errorMessage = await roomsService.RemoveUserFromRoom(request.UserId, request.RoomId);
        if (string.IsNullOrEmpty(errorMessage))
            reply.IsSuccess = true;
        else
            reply.ErrorMessage = errorMessage;
        return reply;
    }
    [Authorize(Policy = "OnlyPrivateClient")]
    public override async Task<GetRoomReply> GetRoom(GetRoomRequest request, ServerCallContext context)
    {
        GetRoomReply reply = new GetRoomReply();
        var room = await roomsService.GetRoom(request.RoomId);
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
            SelectedPlayersCount = room.SelectedPlayersCount,
        };
        reply.Room = replyRoom;
        reply.Members.AddRange(room.Members);
        reply.IsSuccess = true;
        return reply;
    }

}
