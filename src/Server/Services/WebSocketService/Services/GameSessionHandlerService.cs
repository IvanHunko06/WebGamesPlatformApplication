using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SharedApiUtils;
using SharedApiUtils.Interfaces;
using SharedApiUtils.ServicesAccessing.Protos;
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
                    logger.LogInformation($"User {userId} has connected. Remove session task canceled");
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
            logger.LogInformation($"User {userId} has disconnected. Begin End session task");
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
            if (getGameSessionReply is null)
                throw new InternalServerErrorException("getGameSessionReply is null");
            if (!getGameSessionReply.IsSuccess)
                throw new ErrorMessageException(getGameSessionReply.ErrorMessage);

            if (!getGameSessionReply.GameSession.Players.Contains(userId))
                throw new ErrorMessageException(ErrorMessages.NotAllowed);

            SessionInformation sessionInformation = new SessionInformation()
            {
                GameId = getGameSessionReply.GameSession.GameId,
            };
            sessionInformation.Players = new List<string>(getGameSessionReply.GameSession.Players);
            sessionInformation.BeginTime = DateTimeOffset.Parse(getGameSessionReply.GameSession.StartTime);
            sessionInformation.EndTime = !string.IsNullOrEmpty(getGameSessionReply.GameSession.EndTime) ?
                DateTimeOffset.Parse(getGameSessionReply.GameSession.EndTime) : null;

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
            var getGameSessionReply = await gameSessionService.GetGameSession(sessionId);
            if (getGameSessionReply is null)
                throw new InternalServerErrorException("getGameSessionReply is null");
            if (!getGameSessionReply.IsSuccess)
                throw new ErrorMessageException(getGameSessionReply.ErrorMessage);

            if (!getGameSessionReply.GameSession.Players.Contains(userId))
                throw new ErrorMessageException(ErrorMessages.NotAllowed);

            return getGameSessionReply.GameSession.SessionState;
        }catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while retrieving session state");
            throw;
        }
    }
    public async Task<string?> MakeAction(string playerId, string sessionId, string actionName, string payload)
    {
        try
        {
            var sendGameEventReply = await gameSessionService.SendGameEvent(playerId, sessionId, actionName, payload);
            if (sendGameEventReply is null)
                throw new InternalServerErrorException("sendGameEventReply is null");
            logger.LogInformation("Action done for session. Response received.");
            if (!sendGameEventReply.IsSuccess)
                throw new ErrorMessageException(sendGameEventReply.ErrorMessage);

            if (sendGameEventReply.HasNotifyRoomMessage && !string.IsNullOrEmpty(sendGameEventReply.NotifyRoomMessage))
            {
                logger.LogInformation($"Has NotifyMessage for session {sessionId}");
                string? roomId = await serviceInternalRepository.GetSessionRoom(sessionId);
                if (!string.IsNullOrEmpty(roomId))
                {
                    await hubContext.Clients.Group(roomId).ReciveAction(sendGameEventReply.NotifyRoomMessage);
                    logger.LogInformation($"inform the room {roomId} for the session {sessionId} by message {sendGameEventReply.NotifyRoomMessage}");
                }
                else
                {
                    logger.LogInformation("Room id is null");
                }
            }
            return sendGameEventReply.HasNotifyCallerMessage ? sendGameEventReply.NotifyCallerMessage : null;
        }
        catch(Exception ex)
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
            var endGameSessionReply = await gameSessionService.EndGameSession(sessionId, reason, payload);
            if (endGameSessionReply is null)
                throw new InternalServerErrorException("endGameSessionReply");

            if (!endGameSessionReply.IsSuccess)
                throw new ErrorMessageException(endGameSessionReply.ErrorMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while terminating the session");
        }

        try
        {
            string? roomId = await serviceInternalRepository.GetSessionRoom(sessionId);
            if (string.IsNullOrEmpty(roomId))
                throw new InternalServerErrorException("roomId is null");

            var deleteRoomReply = await roomsService.DeleteRoom(roomId);
            if (deleteRoomReply is null)
                throw new InternalServerErrorException("deleteRoomReply");
            if (!deleteRoomReply.IsSuccess && deleteRoomReply.ErrorMessage != ErrorMessages.RoomIdNotExist)
                throw new ErrorMessageException(deleteRoomReply.ErrorMessage);

            await serviceInternalRepository.RemoveSessionRoom(sessionId);
            await hubContext.Clients.Group(roomId).SessionEnded(sessionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while deleting the roon");
        }
    }
}
