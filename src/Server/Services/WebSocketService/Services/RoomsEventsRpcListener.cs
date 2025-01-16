using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;
using WebSocketService.Interfaces;
using WebSocketService.Models;
namespace WebSocketService.Services;

[Authorize(Policy = "OnlyPrivateClient")]
public class RoomsEventsRpcListener : RoomsEvents.RoomsEventsBase
{
    private readonly IRoomsEventsService roomsEvents;

    public RoomsEventsRpcListener(IRoomsEventsService roomsEvents)
    {
        this.roomsEvents = roomsEvents;
    }
    public override async Task<Empty> OnRoomCreated(OnRoomCreatedRequest request, ServerCallContext context)
    {
        Empty empty = new Empty();
        await roomsEvents.InvokedOnRoomCreated(new RoomModel()
        {
            Creator = request.RoomInfo.Creator,
            GameId = request.RoomInfo.GameId,
            IsPrivate = request.RoomInfo.IsPrivate,
            RoomId = request.RoomInfo.RoomId,
            RoomName = request.RoomInfo.RoomName,
            SelectedPlayersCount = request.RoomInfo.SelectedPlayersCount,
            Members = new List<string>()
        });

        return empty;
    }
    public override async Task<Empty> OnRoomDeleated(OnRoomDeleatedRequest request, ServerCallContext context)
    {
        Empty empty = new Empty();
        await roomsEvents.InvokedOnRoomDeleted(new RoomModel()
        {
            Creator = request.RoomInfo.Creator,
            SelectedPlayersCount = request.RoomInfo.SelectedPlayersCount,
            GameId = request.RoomInfo.GameId,
            IsPrivate = request.RoomInfo.IsPrivate,
            RoomId = request.RoomInfo.RoomId,
            RoomName = request.RoomInfo.RoomName,
            Members = new List<string>(request.Members)
        });
        return empty;
    }
    public override Task<Empty> OnRoomJoin(OnRoomJoinRequest request, ServerCallContext context)
    {
        Empty empty = new Empty();
        roomsEvents.InvokedOnRoomJoin(new RoomModel()
        {
            RoomId = request.RoomInfo.RoomId,
            RoomName = request.RoomInfo.RoomName,
            Creator = request.RoomInfo.Creator,
            GameId = request.RoomInfo.GameId,
            IsPrivate = request.RoomInfo.IsPrivate,
            Members = new List<string>(request.Members),
            SelectedPlayersCount = request.RoomInfo.SelectedPlayersCount
        }, request.JoinedMember);
        return Task.FromResult(empty);
    }
    public override Task<Empty> OnRoomLeave(OnRoomLeaveRequest request, ServerCallContext context)
    {
        Empty empty = new Empty();
        roomsEvents.InvokedOnRoomJoin(new RoomModel()
        {
            RoomId = request.RoomInfo.RoomId,
            RoomName = request.RoomInfo.RoomName,
            Creator = request.RoomInfo.Creator,
            GameId = request.RoomInfo.GameId,
            IsPrivate = request.RoomInfo.IsPrivate,
            Members = new List<string>(request.Members),
            SelectedPlayersCount = request.RoomInfo.SelectedPlayersCount
        }, request.RemovedMember);
        return Task.FromResult(empty);
    }
}
