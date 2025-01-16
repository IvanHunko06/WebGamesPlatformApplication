using Microsoft.AspNetCore.SignalR;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using WebSocketService.Clients;
using WebSocketService.Exceptions;
using WebSocketService.Hubs;
using WebSocketService.HubStates;
using WebSocketService.Interfaces;
using WebSocketService.Models;
namespace WebSocketService.Services;

public class GameSessionHandlerService : IGameSessionHandlerService, IDisposable
{
    private readonly IGameSessionServiceClient gameSessionService;
    private readonly ILogger<GameSessionHandlerService> logger;
    private readonly IHubContext<SessionManagmentHub, ISessionManagmentClient> hubContext;
    private readonly IServiceInternalRepository serviceInternalRepository;
    private readonly IRoomsServiceClient roomsService;
    private readonly SessionManagmentHubState hubState;

    public GameSessionHandlerService(IGameSessionServiceClient gameSessionService,
        ILogger<GameSessionHandlerService> logger,
        IHubContext<SessionManagmentHub, ISessionManagmentClient> hubContext,
        IServiceInternalRepository serviceInternalRepository,
        IRoomsServiceClient roomsService,
        SessionManagmentHubState hubState)
    {
        this.gameSessionService = gameSessionService;
        this.logger = logger;
        this.hubContext = hubContext;
        this.serviceInternalRepository = serviceInternalRepository;
        this.roomsService = roomsService;
        this.hubState = hubState;
        hubState.UserConnections.OnUserDisconnected += UserConnections_OnUserDisconnected;
        hubState.UserConnections.OnUserConnected += UserConnections_OnUserConnected;
    }

    private async void UserConnections_OnUserConnected(string userId)
    {
        try
        {
            string? roomId = await serviceInternalRepository.GetUserRoom(userId);


            if (roomId is not null)
            {
                bool? roomIsStarted = await serviceInternalRepository.RoomIsStarted(roomId);
                if (roomIsStarted == false) return;
                string? userConnection = hubState.UserConnections.GetUserConnection(userId);
                if (hubState.UserDisconnectionTokens.TryRemove(userId, out var cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                    logger.LogInformation($"User {userId} has connected. End session task canceled");
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

    public void Dispose()
    {
        hubState.UserConnections.OnUserDisconnected -= UserConnections_OnUserDisconnected;
        hubState.UserConnections.OnUserConnected -= UserConnections_OnUserConnected;
    }
    private async void UserConnections_OnUserDisconnected(string userId)
    {
        try
        {
            string? userRoom = await serviceInternalRepository.GetUserRoom(userId);
            if (string.IsNullOrEmpty(userRoom))
                return;
            bool? isStarted = await serviceInternalRepository.RoomIsStarted(userRoom);
            if (isStarted == false) return;
            logger.LogInformation($"User {userId} has disconnected. Begin end session task");
            var cts = new CancellationTokenSource();
            if (hubState.UserDisconnectionTokens.TryAdd(userId, cts))
            {
                try
                {

                    await Task.Delay(TimeSpan.FromMinutes(2), cts.Token);
                    string? sessionId = await serviceInternalRepository.GetRoomSession(userRoom);
                    if (string.IsNullOrEmpty(sessionId)) return;
                    await EndSession(sessionId, EndSessionReason.PlayerDisconnected, userId);
                    logger.LogInformation($"Session has ended");
                }
                catch (TaskCanceledException)
                {
                    logger.LogInformation($"User {userId} reconnected within 2 minutes");
                }
                finally
                {
                    hubState.UserDisconnectionTokens.TryRemove(userId, out _);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing the task of deleting session");
        }
    }


    public async Task<SessionInformation> GetSessionInformation(string sessionId, string userId)
    {
        try
        {
            var getGameSessionReply = await gameSessionService.GetGameSession(sessionId);
            if (!string.IsNullOrEmpty(getGameSessionReply.errorMessage))
                throw new ErrorMessageException(getGameSessionReply.errorMessage);

            if (!getGameSessionReply.gameSession.Players.Contains(userId))
                throw new ErrorMessageException(ErrorMessages.NotAllowed);

            SessionInformation sessionInformation = new SessionInformation()
            {
                GameId = getGameSessionReply.gameSession.GameId,
            };
            sessionInformation.Players = new List<string>(getGameSessionReply.gameSession.Players);
            sessionInformation.BeginTime = getGameSessionReply.gameSession.StartTime;
            sessionInformation.EndTime = getGameSessionReply.gameSession.EndTime;

            return sessionInformation;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while retrieving session information");
            throw;
        }
    }
    public async Task<string> SyncGameState(string sessionId, string userId)
    {
        try
        {
            var getGameSessionReply = await gameSessionService.SyncGameState(userId, sessionId);
            if (!string.IsNullOrEmpty(getGameSessionReply.errorMessage))
                throw new ErrorMessageException(getGameSessionReply.errorMessage);


            return getGameSessionReply.gameState ?? "";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while retrieving session state");
            throw;
        }
    }
    public async Task<string?> MakeAction(string playerId, string sessionId, string actionName, string payload)
    {
        try
        {
            var sendResult = await gameSessionService.SendGameEvent(playerId, sessionId, actionName, payload);
            if (!string.IsNullOrEmpty(sendResult.errorMessage))
                throw new ErrorMessageException(sendResult.errorMessage);

            return sendResult.gameErrorMessage;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while performing an action for the session");
            throw;
        }

    }
    public async Task EndSession(string sessionId, string reason, string? payload)
    {
        logger.LogInformation($"Ending session {sessionId} with reason {reason}");
        try
        {
            string? errorMessage = await gameSessionService.EndGameSession(sessionId, reason, payload);

            if (!string.IsNullOrEmpty(errorMessage))
                throw new ErrorMessageException(errorMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while terminating the session");
        }

    }
}
