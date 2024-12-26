using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SharedApiUtils.ServicesAccessing.Protos;
using WebSocketService.Clients;
using WebSocketService.Hubs;
using WebSocketService.HubStates;
using WebSocketService.Interfaces;
using WebSocketService.Models;
namespace WebSocketService.Services;

[Authorize(Policy = "OnlyPrivateClient")]
public class RoomsEventsService : RoomsEvents.RoomsEventsBase
{
    private readonly IHubContext<RoomsHub, IRoomsClient> roomsHub;
    private readonly ILogger<RoomsEventsService> logger;
    private readonly IHubContext<SessionManagmentHub, ISessionManagmentClient> sessionManagmentHub;
    private readonly SessionManagmentHubState sessionManagmentHubState;
    private readonly IServiceInternalRepository serviceInternalRepository;

    public RoomsEventsService(IHubContext<RoomsHub, IRoomsClient> roomsHub,
        ILogger<RoomsEventsService> logger,
        IHubContext<SessionManagmentHub, ISessionManagmentClient> sessionManagmentHub,
        SessionManagmentHubState sessionManagmentHubState,
        IServiceInternalRepository serviceInternalRepository)
    {
        this.roomsHub = roomsHub;
        this.logger = logger;
        this.sessionManagmentHub = sessionManagmentHub;
        this.sessionManagmentHubState = sessionManagmentHubState;
        this.serviceInternalRepository = serviceInternalRepository;
    }
    public override async Task<Empty> OnRoomCreated(OnRoomCreatedRequest request, ServerCallContext context)
    {
        Empty empty = new Empty();
        try
        {
            logger.LogInformation("OnRoomCreated event invoked");
            if (!request.RoomInfo.IsPrivate)
            {
                RoomModel roomModel = new RoomModel(request.RoomInfo.RoomId, request.RoomInfo.RoomName, request.RoomInfo.Creator, request.RoomInfo.SelectedPlayersCount, 0);
                _ = Task.Run(() => roomsHub.Clients.Group(request.RoomInfo.GameId).AddRoom(roomModel));
            }
            await serviceInternalRepository.SetRoomIsStarted(request.RoomInfo.RoomId, false);
        }catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while processing the OnRoomCreated event");
        }
        
        return empty;
    }
    public override async Task<Empty> OnRoomDeleated(OnRoomDeleatedRequest request, ServerCallContext context)
    {
        Empty empty = new Empty();
        try
        {

            logger.LogInformation("OnRoomDeleated event invoked");
            if (!request.RoomInfo.IsPrivate)
                _ = Task.Run(() => roomsHub.Clients.Group(request.RoomInfo.GameId).RemoveRoom(request.RoomInfo.RoomId));
            await serviceInternalRepository.RemoveRoomIsStarted(request.RoomInfo.RoomId);
            foreach(var member in request.Members)
            {
                await serviceInternalRepository.DeleteUserRoom(member);
                string? userConnection = sessionManagmentHubState.UserConnections.GetUserConnection(member);
                if (string.IsNullOrEmpty(userConnection)) continue;
                await sessionManagmentHub.Groups.RemoveFromGroupAsync(userConnection, request.RoomInfo.RoomId);
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "an error occurred while processing the OnRoomDeleated event");
        }
        return empty;
    }
    public override Task<Empty> OnRoomJoin(OnRoomJoinRequest request, ServerCallContext context)
    {
        Empty empty = new Empty();
        logger.LogInformation("OnRoomJoin event invoked");
        if (!request.RoomInfo.IsPrivate)
            _ = Task.Run(() => roomsHub.Clients.Group(request.RoomInfo.GameId).UpdateRoom(request.RoomInfo.RoomId, new RoomModel(null, null, null, null, request.RoomInfo.CurrentPlayersCount)));
        return Task.FromResult(empty);
    }
    public override Task<Empty> OnRoomLeave(OnRoomLeaveRequest request, ServerCallContext context)
    {
        Empty empty = new Empty();
        logger.LogInformation("OnRoomLeave event invoked");
        if (!request.RoomInfo.IsPrivate)
            _ = Task.Run(() => roomsHub.Clients.Group(request.RoomInfo.GameId).UpdateRoom(request.RoomInfo.RoomId, new RoomModel(null, null, null, null, request.RoomInfo.CurrentPlayersCount)));
        return Task.FromResult(empty);
    }
}
