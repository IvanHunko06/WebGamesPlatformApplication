using Microsoft.AspNetCore.SignalR;
using SharedApiUtils;
using SharedApiUtils.ServicesAccessing.Connections;
using System.Collections.Concurrent;
using WebSocketService.Clients;
using WebSocketService.Exceptions;
using WebSocketService.Hubs;
using WebSocketService.HubStates;

namespace WebSocketService.Services;

public class RoomSessionHandlerService
{
    private readonly ILogger<RoomSessionHandlerService> logger;
    private readonly IHubContext<SessionManagmentHub, ISessionManagmentClient> hubContext;
    private readonly RoomsServiceConnection roomsService;
    private readonly SessionManagmentHubState hubState;
    private readonly ConcurrentDictionary<string, string> UsersRooms = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _userDisconnectionTokens = new();

    public RoomSessionHandlerService(ILogger<RoomSessionHandlerService> logger,
        IHubContext<SessionManagmentHub, ISessionManagmentClient> hubContext,
        RoomsServiceConnection roomsService,
        SessionManagmentHubState hubState)
    {
        this.logger = logger;
        this.hubContext = hubContext;
        this.roomsService = roomsService;
        this.hubState = hubState;
        hubState.UserConnections.OnUserDisconnected += UserConnections_OnUserDisconnected;
        hubState.UserConnections.OnUserConnected += UserConnections_OnUserConnected;
    }

    private void UserConnections_OnUserConnected(string userId)
    {
        if (_userDisconnectionTokens.TryRemove(userId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            logger.LogInformation($"User {userId} has connected. Leave room task canceled");
        }
    }

    private async void UserConnections_OnUserDisconnected(string userId)
    {
        string? userRoom = GetUserRoom(userId);
        string? userConnection = hubState.UserConnections.GetUserConnection(userId);
        logger.LogInformation($"User {userId} has disconnected. Begin Leave room task");
        if (string.IsNullOrEmpty(userRoom))
            return;
        var cts = new CancellationTokenSource();
        if (_userDisconnectionTokens.TryAdd(userId, cts))
        {
            try
            {
                
                await Task.Delay(TimeSpan.FromSeconds(30), cts.Token);
                
                await RemoveFromRoom(userId, userRoom);

                logger.LogInformation($"User {userId} has been removed from rooms");
            }
            catch (TaskCanceledException)
            {
                userRoom = GetUserRoom(userId);
                userConnection = hubState.UserConnections.GetUserConnection(userId);
                if (string.IsNullOrEmpty(userConnection) || string.IsNullOrEmpty(userRoom))
                    return;

                logger.LogInformation($"User {userId} reconnected within 30 seconds"); 
                await hubContext.Groups.AddToGroupAsync(userConnection, userRoom);
                logger.LogInformation($"Connection {userConnection} added to group {userRoom}");
            }
            finally
            {
                _userDisconnectionTokens.TryRemove(userId, out _);
            }
        }
    }

    public async Task AddToRoom(string userId, string roomId, string accessToken)
    {
        SharedApiUtils.ServicesAccessing.Protos.AddToRoomReply? addToRoomReply = null;
        try
        {
            var combinedClient = await roomsService.GetClient();
            if (combinedClient.client is null || combinedClient.headers is null)
                throw new InternalServerErrorException();

            SharedApiUtils.ServicesAccessing.Protos.AddToRoomRequest request = new()
            {
                AccessToken = accessToken,
                RoomId = roomId,
                UserId = userId
            };
            addToRoomReply = await combinedClient.client.AddToRoomAsync(request, combinedClient.headers);
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex.ToString());
            throw;
        }
        if (addToRoomReply is null)
            throw new InternalServerErrorException("Add to room reply is null");

        if(!addToRoomReply.IsSuccess)
        {
            if(addToRoomReply.ErrorMessage == ErrorMessages.AlreadyInRoom)
                UsersRooms.TryAdd(userId, roomId);

            throw new ErrorMessageException(addToRoomReply.ErrorMessage);
        }

        _ = Task.Run(() => hubContext.Clients.Group(roomId).AddRoomMember(userId));
        UsersRooms.TryAdd(userId, roomId);
        string? userConnection = hubState.UserConnections.GetUserConnection(userId);

        if (string.IsNullOrEmpty(userConnection))
            throw new InternalServerErrorException("user connection is null");

        await hubContext.Groups.AddToGroupAsync(userConnection, roomId);
        logger.LogInformation($"Added {userId} to SignalR hub group {roomId}. User connection: {userConnection}");
    }
    public async Task RemoveFromRoom(string userId, string roomId)
    {
        SharedApiUtils.ServicesAccessing.Protos.RemoveFromRoomReply? removeFromRoomReply = null;
        try
        {
            var combinedClient = await roomsService.GetClient();
            if (combinedClient.client is null || combinedClient.headers is null)
                throw new InternalServerErrorException();

            SharedApiUtils.ServicesAccessing.Protos.RemoveFromRoomRequest request = new()
            {
                RoomId = roomId,
                UserId = userId
            };
            removeFromRoomReply = await combinedClient.client.RemoveFromRoomAsync(request, combinedClient.headers);
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex.ToString());
            throw;
        }
        if (removeFromRoomReply is null)
            throw new InternalServerErrorException("Remove From Room reply is null");

        if (!removeFromRoomReply.IsSuccess)
            throw new ErrorMessageException(removeFromRoomReply.ErrorMessage);

        UsersRooms.TryRemove(userId, out _);
        string? userConnection = hubState.UserConnections.GetUserConnection(userId);

        if (userConnection is not null)
        {
            await hubContext.Groups.RemoveFromGroupAsync(userConnection, roomId);
        }
        _ = Task.Run(() => hubContext.Clients.Group(roomId).RemoveRoomMember(userId));

    }
    public string? GetUserRoom(string userId)
    {
        UsersRooms.TryGetValue(userId, out var room);
        return room;
    }
    public void SetUserRoom(string userId, string roomId)
    {
        UsersRooms.TryAdd(userId, roomId);
    }

    public async Task<SharedApiUtils.ServicesAccessing.Protos.GetRoomReply> GetRoomInformation(string roomId)
    {
        var roomsClientCombination = await roomsService.GetClient();
        SharedApiUtils.ServicesAccessing.Protos.GetRoomReply? getRoomReply = null;
        try
        {
            SharedApiUtils.ServicesAccessing.Protos.GetRoomRequest request = new()
            {
                RoomId = roomId
            };
            if (roomsClientCombination.client is null || roomsClientCombination.headers is null)
            {
                throw new InternalServerErrorException("Rooms client combination is null");
            }
            getRoomReply = await roomsClientCombination.client.GetRoomAsync(request, roomsClientCombination.headers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw;
        }
        if (getRoomReply is null)
        {
            throw new InternalServerErrorException("GetRoom reply is null");
        }
        return getRoomReply;
    }
}
