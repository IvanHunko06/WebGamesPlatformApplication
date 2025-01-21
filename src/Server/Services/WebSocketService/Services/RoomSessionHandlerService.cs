using Microsoft.AspNetCore.SignalR;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Core;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using WebSocketService.Clients;
using WebSocketService.Exceptions;
using WebSocketService.Hubs;
using WebSocketService.HubStates;
using WebSocketService.Interfaces;

namespace WebSocketService.Services;

public class RoomSessionHandlerService : IDisposable, IRoomSessionHandlerService
{
    private readonly ILogger<RoomSessionHandlerService> logger;
    private readonly IHubContext<SessionManagmentHub, ISessionManagmentClient> hubContext;
    private readonly SessionManagmentHubState hubState;
    private readonly IRoomsServiceClient roomsService;
    private readonly IGameSessionServiceClient gameSessionService;
    private readonly IServiceInternalRepository serviceInternalRepository;
    private readonly IGameSessionHandlerService gameSessionHandlerService;

    public RoomSessionHandlerService(ILogger<RoomSessionHandlerService> logger,
        IHubContext<SessionManagmentHub, ISessionManagmentClient> hubContext,
        SessionManagmentHubState hubState,
        IRoomsServiceClient roomsService,
        IGameSessionServiceClient gameSessionService,
        IServiceInternalRepository serviceInternalRepository,
        IGameSessionHandlerService gameSessionHandlerService)
    {
        this.logger = logger;
        this.hubContext = hubContext;
        this.hubState = hubState;
        this.roomsService = roomsService;
        this.gameSessionService = gameSessionService;
        this.serviceInternalRepository = serviceInternalRepository;
        this.gameSessionHandlerService = gameSessionHandlerService;
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
            {
                bool? roomIsStarted = await serviceInternalRepository.RoomIsStarted(roomId);
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
        }
        catch (Exception ex)
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
        }
        catch (Exception ex)
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
            string? errorMessage = await roomsService.AddToRoom(roomId, userId, accessToken);

            if (!string.IsNullOrEmpty(errorMessage))
                throw new ErrorMessageException(errorMessage);

            await hubContext.Clients.Group(roomId).AddRoomMember(userId);
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
            string? errorMessage = await roomsService.RemoveFromRoom(roomId, userId);

            if (!string.IsNullOrEmpty(errorMessage))
                throw new ErrorMessageException(errorMessage);

            await serviceInternalRepository.DeleteUserRoom(userId);
            string? userConnection = hubState.UserConnections.GetUserConnection(userId);

            if (userConnection is not null)
            {
                await hubContext.Groups.RemoveFromGroupAsync(userConnection, roomId);
            }
            _ = Task.Run(() => hubContext.Clients.Group(roomId).RemoveRoomMember(userId));
            _ = Task.Run(async () =>
            {
                string? sessionId = await serviceInternalRepository.GetRoomSession(roomId);
                if (string.IsNullOrEmpty(sessionId))
                {
                    logger.LogWarning($"Room {roomId} session is null");
                    return;
                }
                await gameSessionHandlerService.EndSession(sessionId, EndSessionReason.PlayerDisconnected, userId);
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while removing the user from the room");
            throw;
        }


    }

    public async Task<RoomModelDto> GetRoomInformation(string roomId)
    {
        try
        {
            var getRoomReply = await roomsService.GetRoom(roomId);
            if (!string.IsNullOrEmpty(getRoomReply.errorMessage))
            {
                throw new ErrorMessageException(getRoomReply.errorMessage);
            }
            return getRoomReply.roomModel;
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
            var getRoomReply = await roomsService.GetRoom(roomId);

            if (!string.IsNullOrEmpty(getRoomReply.errorMessage))
                throw new ErrorMessageException(getRoomReply.errorMessage);

            if (getRoomReply.roomModel?.Creator != userId)
                throw new ErrorMessageException(ErrorMessages.NotAllowed);

            if (getRoomReply.roomModel.SelectedPlayerCount != getRoomReply.roomModel.Members.Count)
                throw new ErrorMessageException(ErrorMessages.RoomIsNotFull);

            var startSessionReply = await gameSessionService.StartGameSession(roomId);

            if (!string.IsNullOrEmpty(startSessionReply.errorMessage))
                throw new ErrorMessageException(startSessionReply.errorMessage);

            await hubContext.Clients.Group(roomId).GameStarted(startSessionReply.sessionId ?? "");
            await serviceInternalRepository.SetRoomIsStarted(roomId, true);
            await serviceInternalRepository.SetSessionRoom(startSessionReply.sessionId ?? "", roomId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while starting the game");
            throw;
        }

    }
}
