using RoomManagmentService.Models;
using RoomsService.Interfaces;
using SharedApiUtils.gRPC.ServicesAccessing.Connections;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;
namespace RoomsService.Services;

public class RpcRoomEventNotifier : IRoomEventNotifier
{
    private readonly RoomsEventsConnection roomsEvents;
    private readonly ILogger<RpcRoomEventNotifier> logger;

    public RpcRoomEventNotifier(RoomsEventsConnection roomsEvents, ILogger<RpcRoomEventNotifier> logger)
    {
        this.roomsEvents = roomsEvents;
        this.logger = logger;
    }

    public async Task NotifyRoomCreated(RoomModel room)
    {
        try
        {
            RoomEventRoomInfo roomEventInfo = new RoomEventRoomInfo()
            {
                Creator = room.Creator,
                CurrentPlayersCount = room.CurrentPlayersCount,
                SelectedPlayersCount = room.SelectedPlayersCount,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomId = room.RoomId,
                RoomName = room.RoomName,
            };
            var clientCombination = await roomsEvents.GetClient();
            var request = new OnRoomCreatedRequest()
            {
                RoomInfo = roomEventInfo,
            };
            await clientCombination.client.OnRoomCreatedAsync(request, clientCombination.headers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while notifying the OnRoomCreated event");
        }
    }
    public async Task NotifyRoomDeleted(RoomModel room)
    {
        try
        {
            RoomEventRoomInfo roomEventInfo = new RoomEventRoomInfo()
            {
                Creator = room.Creator,
                CurrentPlayersCount = room.CurrentPlayersCount,
                SelectedPlayersCount = room.SelectedPlayersCount,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomId = room.RoomId,
                RoomName = room.RoomName,
            };
            var clientCombination = await roomsEvents.GetClient();
            var request = new OnRoomDeleatedRequest()
            {
                RoomInfo = roomEventInfo
            };
            request.Members.AddRange(room.Members);
            var reply = await clientCombination.client.OnRoomDeleatedAsync(request, clientCombination.headers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while notifying the OnRoomDeleted event");
        }
    }
    public async Task NotifyRoomJoin(RoomModel room, string joinedMember)
    {
        try
        {
            RoomEventRoomInfo roomEventInfo = new RoomEventRoomInfo()
            {
                Creator = room.Creator,
                CurrentPlayersCount = room.CurrentPlayersCount,
                SelectedPlayersCount = room.SelectedPlayersCount,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomId = room.RoomId,
                RoomName = room.RoomName,
            };
            var clientCombination = await roomsEvents.GetClient();
            var request = new OnRoomJoinRequest()
            {
                RoomInfo = roomEventInfo,
                JoinedMember = joinedMember,
            };
            request.Members.AddRange(room.Members);
            var reply = await clientCombination.client.OnRoomJoinAsync(request, clientCombination.headers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while notifying the OnRoomJoin event");
        }
    }
    public async Task NotifyRoomLeave(RoomModel room, string removedMember)
    {
        try
        {
            RoomEventRoomInfo roomEventInfo = new RoomEventRoomInfo()
            {
                Creator = room.Creator,
                CurrentPlayersCount = room.CurrentPlayersCount,
                SelectedPlayersCount = room.SelectedPlayersCount,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomId = room.RoomId,
                RoomName = room.RoomName,
            };
            var clientCombination = await roomsEvents.GetClient();
            var request = new OnRoomLeaveRequest()
            {
                RoomInfo = roomEventInfo,
                RemovedMember = removedMember,
            };
            request.Members.AddRange(room.Members);
            var reply = await clientCombination.client.OnRoomLeaveAsync(request, clientCombination.headers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while notifying the OnRoomLeave event");
        }
    }
}
