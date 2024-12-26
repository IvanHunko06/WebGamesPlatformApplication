using Microsoft.AspNetCore.SignalR;
using SharedApiUtils;
using SharedApiUtils.Interfaces;
using SharedApiUtils.ServicesAccessing.Connections;
using SharedApiUtils.ServicesAccessing.Protos;
using System.Collections.Concurrent;
using WebSocketService.Clients;
using WebSocketService.Exceptions;
using WebSocketService.Hubs;
using WebSocketService.HubStates;
using WebSocketService.Interfaces;

namespace WebSocketService.Services;

public class RoomSessionHandlerService :IDisposable, IRoomSessionHandlerService
{
    private readonly ILogger<RoomSessionHandlerService> logger;
    private readonly IHubContext<SessionManagmentHub, ISessionManagmentClient> hubContext;
    private readonly SessionManagmentHubState hubState;
    private readonly IRoomsServiceClient roomsService;
    private readonly IGameSessionServiceClient gameSessionService;
    private readonly IServiceInternalRepository serviceInternalRepository;
    

    public RoomSessionHandlerService(ILogger<RoomSessionHandlerService> logger,
        IHubContext<SessionManagmentHub, ISessionManagmentClient> hubContext,
        SessionManagmentHubState hubState,
        IRoomsServiceClient roomsService,
        IGameSessionServiceClient gameSessionService,
        IServiceInternalRepository serviceInternalRepository)
    {
        this.logger = logger;
        this.hubContext = hubContext;
        this.hubState = hubState;
        this.roomsService = roomsService;
        this.gameSessionService = gameSessionService;
        this.serviceInternalRepository = serviceInternalRepository;
        hubState.UserConnections.OnUserDisconnected += UserConnections_OnUserDisconnected;
        hubState.UserConnections.OnUserConnected += UserConnections_OnUserConnected;
    }
    public void Dispose()
    {
        hubState.UserConnections.OnUserDisconnected -= UserConnections_OnUserDisconnected;
        hubState.UserConnections.OnUserConnected -= UserConnections_OnUserConnected;
    } 

    private async void UserConnections_OnUserConnected(string userId)
    {
        try
        {
            string? roomId = await serviceInternalRepository.GetUserRoom(userId);
            
            
            if (roomId is not null)
            { bool? roomIsStarted = await serviceInternalRepository.RoomIsStarted(roomId);
                if (roomIsStarted == true) return; 
                string? userConnection = hubState.UserConnections.GetUserConnection(userId);
                if (hubState.UserDisconnectionTokens.TryRemove(userId, out var cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                    logger.LogInformation($"User {userId} has connected. Leave room task canceled");
                }
                if (userConnection is not null)
                {
                    await hubContext.Groups.AddToGroupAsync(userConnection, roomId);
                    logger.LogInformation($"Connection {userConnection} added to group {roomId}");
                }
            }
        }catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred during the OnUserConnected event");
        }
        
    }

    private async void UserConnections_OnUserDisconnected(string userId)
    {
        try
        {
            string? userRoom = await serviceInternalRepository.GetUserRoom(userId);
            if (string.IsNullOrEmpty(userRoom))
                return;
            bool? isStarted = await serviceInternalRepository.RoomIsStarted(userRoom);
            if (isStarted == true) return;
            logger.LogInformation($"User {userId} has disconnected. Begin Leave room task");
            var cts = new CancellationTokenSource();
            if (hubState.UserDisconnectionTokens.TryAdd(userId, cts))
            {
                try
                {

                    await Task.Delay(TimeSpan.FromSeconds(30), cts.Token);
                    await RemoveFromRoom(userId, userRoom);
                    logger.LogInformation($"User {userId} has been removed from rooms");
                }
                catch (TaskCanceledException)
                {
                    logger.LogInformation($"User {userId} reconnected within 30 seconds");
                }
                finally
                {
                    hubState.UserDisconnectionTokens.TryRemove(userId, out _);
                }
            }
        }catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing the task of removing a user from a room");
        }
        
    }

    public async Task AddToRoom(string userId, string roomId, string accessToken)
    {

        try
        {
            bool? roomIsStarted = await serviceInternalRepository.RoomIsStarted(roomId);
            if (roomIsStarted is not null && roomIsStarted == true)
                throw new ErrorMessageException(ErrorMessages.GameInProgress);
            AddToRoomReply? addToRoomReply = await roomsService.AddToRoom(roomId, userId, accessToken);
            if (addToRoomReply is null)
                throw new InternalServerErrorException("Add to room reply is null");

            if (!addToRoomReply.IsSuccess)
                throw new ErrorMessageException(addToRoomReply.ErrorMessage);

            _ = Task.Run(() => hubContext.Clients.Group(roomId).AddRoomMember(userId));
            await serviceInternalRepository.SetUserRoom(userId, roomId);
            string? userConnection = hubState.UserConnections.GetUserConnection(userId);

            if (string.IsNullOrEmpty(userConnection))
                throw new InternalServerErrorException("user connection is null");

            await hubContext.Groups.AddToGroupAsync(userConnection, roomId);
            logger.LogInformation($"Added {userId} to SignalR hub group {roomId}. User connection: {userConnection}");
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "An error occurred while adding a user to the room");
            throw;
        }
        
    }
    public async Task RemoveFromRoom(string userId, string roomId)
    {
        try
        {
            RemoveFromRoomReply? removeFromRoomReply = await roomsService.RemoveFromRoom(roomId, userId);
            if (removeFromRoomReply is null)
                throw new InternalServerErrorException("Remove From Room reply is null");

            if (!removeFromRoomReply.IsSuccess)
                throw new ErrorMessageException(removeFromRoomReply.ErrorMessage);

            //hubState.UsersRooms.TryRemove(userId, out _);
            await serviceInternalRepository.DeleteUserRoom(userId);
            string? userConnection = hubState.UserConnections.GetUserConnection(userId);

            if (userConnection is not null)
            {
                await hubContext.Groups.RemoveFromGroupAsync(userConnection, roomId);
            }
            _ = Task.Run(() => hubContext.Clients.Group(roomId).RemoveRoomMember(userId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while removing the user from the room");
            throw;
        }
        

    }

    public async Task<GetRoomReply> GetRoomInformation(string roomId)
    {
        try
        {
            GetRoomReply? getRoomReply = await roomsService.GetRoom(roomId);
            if (getRoomReply is null)
            {
                throw new InternalServerErrorException("GetRoom reply is null");
            }
            return getRoomReply;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw;
        }
        
    }

    public async Task StartGame(string roomId, string userId)
    {
        try
        {
            GetRoomReply? getRoomReply = await roomsService.GetRoom(roomId);
            if (getRoomReply is null)
                throw new NullReferenceException("get room reply is null");

            if (!getRoomReply.IsSuccess)
                throw new ErrorMessageException(getRoomReply.ErrorMessage);

            if (getRoomReply.Room.Creator != userId)
                throw new ErrorMessageException(ErrorMessages.NotAllowed);

            if (getRoomReply.Room.SelectedPlayersCount != getRoomReply.Room.CurrentPlayersCount)
                throw new ErrorMessageException(ErrorMessages.RoomIsNotFull);

            var startSessionReply = await gameSessionService.StartGameSession(roomId, getRoomReply.Members);
            if (startSessionReply is null)
                throw new NullReferenceException("start session reply is null");

            if (!startSessionReply.IsSuccess)
                throw new ErrorMessageException(startSessionReply.ErrorMessage);

            await hubContext.Clients.Group(roomId).GameStarted(startSessionReply.SessionId);
            await serviceInternalRepository.SetRoomIsStarted(roomId, true);
            await serviceInternalRepository.SetSessionRoom(startSessionReply.SessionId, roomId);
        }catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while starting the game");
            throw;
        }
        
    }
}
