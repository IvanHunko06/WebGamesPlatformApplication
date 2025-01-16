using Microsoft.AspNetCore.SignalR;
using WebSocketService.Clients;
using WebSocketService.Hubs;
using WebSocketService.HubStates;
using WebSocketService.Interfaces;

namespace WebSocketService.Services;

public class GameSessionWsNotifyer : IGameSessionWsNotifyer
{
    private readonly IHubContext<SessionManagmentHub, ISessionManagmentClient> sessionManagmentHub;
    private readonly IServiceInternalRepository serviceInternalRepository;
    private readonly ILogger<GameSessionWsNotifyer> logger;
    private readonly SessionManagmentHubState sessionManagmentHubState;

    public GameSessionWsNotifyer(
        IHubContext<SessionManagmentHub, ISessionManagmentClient> sessionManagmentHub,
        IServiceInternalRepository serviceInternalRepository,
        ILogger<GameSessionWsNotifyer> logger,
        SessionManagmentHubState sessionManagmentHubState)
    {
        this.sessionManagmentHub = sessionManagmentHub;
        this.serviceInternalRepository = serviceInternalRepository;
        this.logger = logger;
        this.sessionManagmentHubState = sessionManagmentHubState;
    }
    public async Task NotifyReciveAction_Room(string sessionId, string message)
    {
        try
        {
            string? roomId = await serviceInternalRepository.GetSessionRoom(sessionId);
            if (string.IsNullOrEmpty(roomId))
            {
                logger.LogWarning($"Session {sessionId} room not found");
                return;
            }
            await sessionManagmentHub.Clients.Group(roomId).ReciveAction(message);
            logger.LogInformation($"Room {roomId} is notified with a message {message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"an error occurred while notifying the room");
        }
    }
    public async Task NotifyReciveAction_User(string sessionId, string userId, string message)
    {
        try
        {

            string? roomId = await serviceInternalRepository.GetSessionRoom(sessionId);
            string? userRoom = await serviceInternalRepository.GetUserRoom(userId);
            if (string.IsNullOrEmpty(roomId))
            {
                logger.LogWarning($"Session {sessionId} room is null");
                return;
            }
            if (string.IsNullOrEmpty(userRoom))
            {
                logger.LogWarning($"User {userId} room is null");
                return;
            }
            if (userRoom != roomId)
            {
                logger.LogWarning($"user {userId} room {userRoom} does not match session {sessionId} room {roomId}");
                return;
            }
            string? userConnection = sessionManagmentHubState.UserConnections.GetUserConnection(userId);
            if (string.IsNullOrEmpty(userConnection))
            {
                logger.LogWarning($"User {userId} connection is null");
                return;
            }
            await sessionManagmentHub.Clients.Client(userConnection).ReciveAction(message);
            logger.LogInformation($"User {userId} is notified with a message {message}. UserConnection: {userConnection}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"an error occurred while notifying the user {userId}");
        }
    }
    public async Task NotifySessionEnded_User(string sessionId, string userId, string reason, string? payload)
    {
        try
        {

            string? roomId = await serviceInternalRepository.GetSessionRoom(sessionId);
            string? userRoom = await serviceInternalRepository.GetUserRoom(userId);
            if (string.IsNullOrEmpty(roomId))
            {
                logger.LogWarning($"Session {sessionId} room is null");
                return;
            }
            if (string.IsNullOrEmpty(userRoom))
            {
                logger.LogWarning($"User {userId} room is null");
                return;
            }
            if (userRoom != roomId)
            {
                logger.LogWarning($"user {userId} room {userRoom} does not match session {sessionId} room {roomId}");
                return;
            }
            string? userConnection = sessionManagmentHubState.UserConnections.GetUserConnection(userId);
            if (string.IsNullOrEmpty(userConnection))
            {
                logger.LogWarning($"User {userId} connection is null");
                return;
            }
            await sessionManagmentHub.Clients.Client(userConnection).SessionEnded(reason, payload);
            logger.LogInformation($"Game ended. User {userId} Reason: {reason}. Payload: {payload}. UserConnection: {userConnection}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"an error occurred while notifying the user {userId}");
        }
    }
    public async Task NotifySessionEnded_Room(string sessionId, string reason, string? payload)
    {
        try
        {
            string? roomId = await serviceInternalRepository.GetSessionRoom(sessionId);
            if (string.IsNullOrEmpty(roomId))
            {
                logger.LogWarning($"Session {sessionId} room not found");
                return;
            }
            await sessionManagmentHub.Clients.Group(roomId).SessionEnded(reason, payload);
            logger.LogInformation($"Room {roomId} session ended event invoked. Reason: {reason}. Payload: {payload}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"an error occurred while notifying the room");
        }
    }
}
