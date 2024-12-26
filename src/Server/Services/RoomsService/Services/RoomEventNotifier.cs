using RoomManagmentService.Models;
using SharedApiUtils.ServicesAccessing.Connections;
using SharedApiUtils.ServicesAccessing.Protos;
namespace RoomsService.Services;

public class RoomEventNotifier
{
    private readonly RoomsEventsConnection roomsEvents;
    private readonly ILogger<RoomEventNotifier> logger;

    public RoomEventNotifier(RoomsEventsConnection roomsEvents, ILogger<RoomEventNotifier> logger)
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
                SelectedPlayersCount = room.SelectedPlayerCount,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomId = room.RoomId,
                RoomName = room.RoomName,
            };
            var clientCombination = await roomsEvents.GetClient();
            var request = new SharedApiUtils.ServicesAccessing.Protos.OnRoomCreatedRequest()
            {
                RoomInfo = roomEventInfo,
            };
            request.Members.AddRange(room.Members);
            await clientCombination.client.OnRoomCreatedAsync(request, clientCombination.headers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
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
                SelectedPlayersCount = room.SelectedPlayerCount,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomId = room.RoomId,
                RoomName = room.RoomName,
            };
            var clientCombination = await roomsEvents.GetClient();
            var request = new SharedApiUtils.ServicesAccessing.Protos.OnRoomDeleatedRequest()
            {
                RoomInfo = roomEventInfo
            };
            request.Members.AddRange(room.Members);
            var reply = await clientCombination.client.OnRoomDeleatedAsync(request, clientCombination.headers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
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
                SelectedPlayersCount = room.SelectedPlayerCount,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomId = room.RoomId,
                RoomName = room.RoomName,
            };
            var clientCombination = await roomsEvents.GetClient();
            var request = new SharedApiUtils.ServicesAccessing.Protos.OnRoomJoinRequest()
            {
                RoomInfo = roomEventInfo,
                JoinedMember = joinedMember,
            };
            request.Members.AddRange(room.Members);
            var reply = await clientCombination.client.OnRoomJoinAsync(request, clientCombination.headers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
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
                SelectedPlayersCount = room.SelectedPlayerCount,
                GameId = room.GameId,
                IsPrivate = room.IsPrivate,
                RoomId = room.RoomId,
                RoomName = room.RoomName,
            };
            var clientCombination = await roomsEvents.GetClient();
            var request = new SharedApiUtils.ServicesAccessing.Protos.OnRoomLeaveRequest()
            {
                RoomInfo = roomEventInfo,
                RemovedMember = removedMember,
            };
            request.Members.AddRange(room.Members);
            var reply = await clientCombination.client.OnRoomLeaveAsync(request, clientCombination.headers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
        }
    }
}
