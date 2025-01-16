using Microsoft.AspNetCore.SignalR;
using WebSocketService.Clients;
using WebSocketService.Hubs;
using WebSocketService.HubStates;
using WebSocketService.Interfaces;
using WebSocketService.Models;

namespace WebSocketService.Services;

public class RoomsEventsService : IRoomsEventsService
{
    private readonly ILogger<RoomsEventsService> logger;
    private readonly IHubContext<SessionManagmentHub, ISessionManagmentClient> sessionManagmentHub;
    private readonly IHubContext<RoomsHub, IRoomsClient> roomsHub;
    private readonly IServiceInternalRepository serviceInternalRepository;
    private readonly SessionManagmentHubState sessionManagmentHubState;

    public RoomsEventsService(
        ILogger<RoomsEventsService> logger,
        IHubContext<SessionManagmentHub, ISessionManagmentClient> sessionManagmentHub,
        IHubContext<RoomsHub, IRoomsClient> roomsHub,
        IServiceInternalRepository serviceInternalRepository,
        SessionManagmentHubState sessionManagmentHubState)
    {
        this.logger = logger;
        this.sessionManagmentHub = sessionManagmentHub;
        this.roomsHub = roomsHub;
        this.serviceInternalRepository = serviceInternalRepository;
        this.sessionManagmentHubState = sessionManagmentHubState;
    }
    public async Task InvokedOnRoomCreated(RoomModel room)
    {
        try
        {
            logger.LogInformation("OnRoomCreated event invoked");
            if (room.IsPrivate)
            {
                RoomClientModel roomModel = new RoomClientModel(room.RoomId, room.RoomName, room.Creator, room.SelectedPlayersCount, 0);
                _ = Task.Run(() => roomsHub.Clients.Group(room.GameId).AddRoom(roomModel));
            }
            await serviceInternalRepository.SetRoomIsStarted(room.RoomId, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while processing the OnRoomCreated event");
        }
    }
    public async Task InvokedOnRoomDeleted(RoomModel room)
    {
        try
        {

            logger.LogInformation("OnRoomDeleated event invoked");
            if (!room.IsPrivate)
                _ = Task.Run(() => roomsHub.Clients.Group(room.GameId).RemoveRoom(room.RoomId));
            await serviceInternalRepository.RemoveRoomIsStarted(room.RoomId);
            foreach (var member in room.Members)
            {
                await serviceInternalRepository.DeleteUserRoom(member);
                string? userConnection = sessionManagmentHubState.UserConnections.GetUserConnection(member);
                if (string.IsNullOrEmpty(userConnection)) continue;
                await sessionManagmentHub.Groups.RemoveFromGroupAsync(userConnection, room.RoomId);
            }
            string? sessionId = await serviceInternalRepository.GetRoomSession(room.RoomId);
            if(!string.IsNullOrEmpty(sessionId))
                await serviceInternalRepository.RemoveSessionRoom(sessionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while processing the OnRoomDeleated event");
        }
    }
    public Task InvokedOnRoomJoin(RoomModel room, string joinedMember)
    {
        try
        {
            logger.LogInformation("OnRoomJoin event invoked");
            if (!room.IsPrivate)
                _ = Task.Run(() => roomsHub.Clients.Group(room.GameId).UpdateRoom(room.RoomId, new RoomClientModel(null, null, null, null, room.Members.Count)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while processing the OnRoomJoin event");
        }
        return Task.CompletedTask;
    }
    public Task InvokedOnRoomLeave(RoomModel room, string deletedMember)
    {
        try
        {
            logger.LogInformation("OnRoomLeave event invoked");
            if (!room.IsPrivate)
                _ = Task.Run(() => roomsHub.Clients.Group(room.GameId).UpdateRoom(room.RoomId, new RoomClientModel(null, null, null, null, room.Members.Count)));

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while processing the OnRoomLeave event");
        }
        return Task.CompletedTask;
    }
}
