using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SharedApiUtils;
using SharedApiUtils.ServicesAccessing.Connections;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json;
using WebSocketService.Clients;
using WebSocketService.Exceptions;
using WebSocketService.HubStates;
using WebSocketService.Interfaces;
using WebSocketService.Models;
using WebSocketService.Services;

namespace WebSocketService.Hubs;

[Authorize(Policy = "OnlyPublicClient")]
public class SessionManagmentHub : Hub<ISessionManagmentClient>
{
    private readonly ILogger<SessionManagmentHub> logger;
    private readonly SessionManagmentHubState hubState;
    private readonly IRoomSessionHandlerService roomSessionHandlerService;
    private readonly IUserContextService userContext;
    private readonly IServiceInternalRepository serviceInternalRepository;
    private readonly IGameSessionHandlerService gameSessionHandlerService;

    public SessionManagmentHub(ILogger<SessionManagmentHub> logger, 
        SessionManagmentHubState hubState, 
        IRoomSessionHandlerService roomSessionHandlerService, 
        IUserContextService userContext,
        IServiceInternalRepository serviceInternalRepository,
        IGameSessionHandlerService gameSessionHandlerService)
    {
        this.logger = logger;
        this.hubState = hubState;
        this.roomSessionHandlerService = roomSessionHandlerService;
        this.userContext = userContext;
        this.serviceInternalRepository = serviceInternalRepository;
        this.gameSessionHandlerService = gameSessionHandlerService;
    }


    public override async Task OnConnectedAsync()
    {
        string? userId = userContext.GetUserId(Context);
        if (string.IsNullOrEmpty(userId))
        {
            Context.Abort();
            return;
        }
        if (!string.IsNullOrEmpty(hubState.UserConnections.GetUserConnection(userId)))
        {
            Context.Abort();
            return;
        }
        logger.LogInformation($"Client {Context.ConnectionId} has connected to SessionManagmentHub");
        hubState.UserConnections.AddOrSetUserConnectionId(userId, Context.ConnectionId);
        hubState.ConnectedClients.Add(Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (!hubState.ConnectedClients.Contains(Context.ConnectionId))
            return;
        logger.LogInformation($"Client {Context.ConnectionId} has disconnected from SessionManagmentHub");
        string? userId = userContext.GetUserId(Context);
        if (string.IsNullOrEmpty(userId))
            return;
        hubState.ConnectedClients.Remove(Context.ConnectionId);
        hubState.UserConnections.RemoveUserConnection(userId, Context.ConnectionId);
    }
    public async Task<HubActionResult> JoinRoom(string roomId, string accessToken)
    {   
        try
        {
            logger.LogInformation($"{Context.ConnectionId}: join room  request to room {roomId} with access token {accessToken}");
            string? userId = userContext.GetUserId(Context);
            if (userId is null)
                return new HubActionResult(false, ErrorMessages.SubjectClaimNotFound, null);

            if (!string.IsNullOrEmpty(await serviceInternalRepository.GetUserRoom(userId)))
                return new HubActionResult(false, ErrorMessages.UserAsignedToRoom, null);
            await roomSessionHandlerService.AddToRoom(userId, roomId, accessToken);
            return new HubActionResult(true, null, null);
        }
        catch (ErrorMessageException ex)
        {
            return new HubActionResult(false, ex.Message, null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.ToString());
            return new HubActionResult(false, ErrorMessages.InternalServerError, null);
        }
    }
    public async Task<HubActionResult> GetRoomInformation(string roomId)
    {
        logger.LogInformation($"{Context.ConnectionId}: GetRoomInformation  request to room {roomId}");
        string? userId = userContext.GetUserId(Context);
        if (string.IsNullOrEmpty(userId))
        {
            return new HubActionResult(false, ErrorMessages.SubjectClaimNotFound, null);
        }
        try
        {
            var getRoomReply = await roomSessionHandlerService.GetRoomInformation(roomId);
            HubActionResult reply = new HubActionResult(false, null, null);
            if (!getRoomReply.IsSuccess)
            {
                reply.ErrorMessage = getRoomReply.ErrorMessage;
                return reply;
            }
            if (!getRoomReply.Members.Contains(userId))
            {
                reply.ErrorMessage = ErrorMessages.NotAllowed;
                return reply;
            }
            reply.IsSuccess = true;
            reply.Payload = new
            {
                Members = new List<string>(getRoomReply.Members),
                Room = new Models.RoomModel
                (getRoomReply.Room.RoomId,
                getRoomReply.Room.RoomName,
                getRoomReply.Room.Creator,
                getRoomReply.Room.SelectedPlayersCount,
                getRoomReply.Members.Count)
            };
            return reply;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex.ToString());
            return new HubActionResult(false, ErrorMessages.InternalServerError, null);
        }


    }
    public async Task<HubActionResult> LeaveRoom(string roomId)
    {
        logger.LogInformation($"{Context.ConnectionId}: leave room  request from room {roomId}");
        string? userId = userContext.GetUserId(Context);
        if (string.IsNullOrEmpty(userId))
            return new HubActionResult(false, ErrorMessages.SubjectClaimNotFound, null);

        try
        {
            await roomSessionHandlerService.RemoveFromRoom(userId, roomId);
            return new HubActionResult(true, null, null);
        }
        catch (ErrorMessageException ex)
        {
            return new HubActionResult(false, ex.Message, null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.ToString());
            return new HubActionResult(false, ErrorMessages.InternalServerError, null);
        }

    }
    public async Task<HubActionResult> StartGame(string roomId)
    {
        logger.LogInformation($"{Context.ConnectionId}: start game request for room {roomId}");
        string? userId = userContext.GetUserId(Context);
        if (string.IsNullOrEmpty(userId))
            return new HubActionResult(false, ErrorMessages.SubjectClaimNotFound, null);
        try
        {
            await roomSessionHandlerService.StartGame(roomId, userId);
            return new HubActionResult(true, null, null);
        }catch(ErrorMessageException ex)
        {
            return new HubActionResult(false, ex.Message, null);
        }
        catch(Exception)
        {
            return new HubActionResult(false, ErrorMessages.InternalServerError, null);
        }
    }
    public async Task<HubActionResult> GetSessionInformation(string sessionId)
    {
        try
        {
            logger.LogInformation($"GetSessionInformation request for session {sessionId}");
            string? userId = userContext.GetUserId(Context);
            if (string.IsNullOrEmpty(userId))
                return new HubActionResult(false, ErrorMessages.SubjectClaimNotFound, null);

            SessionInformation sessionInformation = await gameSessionHandlerService.GetSessionInformation(sessionId, userId);
            return new HubActionResult(true, null, sessionInformation);
        }catch (ErrorMessageException ex)
        {
            return new HubActionResult(false, ex.Message, null);
        }
        catch(Exception)
        {
            return new HubActionResult(false, ErrorMessages.InternalServerError, null);
        }
    }
    public async Task<HubActionResult> SyncGameState(string sessionId)
    {
        try
        {
            logger.LogInformation($"SyncGameState request for session {sessionId}");
            string? userId = userContext.GetUserId(Context);
            if (string.IsNullOrEmpty(userId))
                return new HubActionResult(false, ErrorMessages.SubjectClaimNotFound, null);
            string gameState = await gameSessionHandlerService.SyncGameState(sessionId, userId);
            return new HubActionResult(true, null, gameState);
        }
        catch(ErrorMessageException ex)
        {
            return new HubActionResult(false, ex.Message, null);
        }
        catch (Exception)
        {
            return new HubActionResult(false, ErrorMessages.InternalServerError, null);
        }
    }
    public async Task<HubActionResult> MakeAction(string sessionId, string action, string payload)
    {
        try
        {
            logger.LogInformation($"SyncGameState request for session {sessionId}");
            string? userId = userContext.GetUserId(Context);
            if (string.IsNullOrEmpty(userId))
                return new HubActionResult(false, ErrorMessages.SubjectClaimNotFound, null);
            string? notifyCallerMessage = await gameSessionHandlerService.MakeAction(userId, sessionId, action, payload);
            return new HubActionResult(true, null, notifyCallerMessage);
        }
        catch(ErrorMessageException ex)
        {
            return new HubActionResult(false, ex.Message, null);
        }
        catch (Exception)
        {
            return new HubActionResult(false, ErrorMessages.InternalServerError, null);
        }

    }
}
