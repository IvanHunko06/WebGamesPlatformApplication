using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebSocketService.Clients;
using WebSocketService.Hubs;
using WebSocketService.Models;
using WebSocketService.Protos;
namespace WebSocketService.Services;

[Authorize(Policy = "OnlyPrivateClient")]
public class RoomsEventsService : RoomsEvents.RoomsEventsBase
{
    private readonly IHubContext<RoomsHub, IRoomsClient> roomsHub;
    private readonly ILogger<RoomsEventsService> logger;

    public RoomsEventsService(IHubContext<RoomsHub, IRoomsClient> roomsHub, ILogger<RoomsEventsService> logger)
    {
        this.roomsHub = roomsHub;
        this.logger = logger;
    }
    public override Task<Empty> OnRoomCreated(OnRoomCreatedRequest request, ServerCallContext context)
    {
        Empty empty = new Empty();
        logger.LogInformation("OnRoomCreated event invoked");
        RoomModel roomModel = new RoomModel(request.RoomId, request.RoomName, request.Creator, request.SelectedPlayerCount, 0);
        Task.Run(() => roomsHub.Clients.Group(request.GameId).AddRoom(roomModel));
        return Task.FromResult(empty);
    }
    public override Task<Empty> OnRoomDeleated(OnRoomDeleatedRequest request, ServerCallContext context)
    {
        Empty empty = new Empty();
        logger.LogInformation("OnRoomDeleated event invoked");
        _ = Task.Run(() => roomsHub.Clients.Group(request.GameId).RemoveRoom(request.RoomId));
        return Task.FromResult(empty);
    }
    public override Task<Empty> OnRoomJoin(OnRoomJoinRequest request, ServerCallContext context)
    {
        Empty empty = new Empty();
        logger.LogInformation("OnRoomLeave event invoked");
        _ = Task.Run(() => roomsHub.Clients.Group(request.GameId).UpdateRoom(request.RoomId, new RoomModel(null, null, null, null, request.MembersCount)));
        return Task.FromResult(empty);
    }
    public override Task<Empty> OnRoomLeave(OnRoomLeaveRequest request, ServerCallContext context)
    {
        Empty empty = new Empty();
        logger.LogInformation("OnRoomLeave event invoked");
        _ = Task.Run(()=>roomsHub.Clients.Group(request.GameId).UpdateRoom(request.RoomId, new RoomModel(null, null, null, null, request.MembersCount)));
        return Task.FromResult(empty);
    }
}
