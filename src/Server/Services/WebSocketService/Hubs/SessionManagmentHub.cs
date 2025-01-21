using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SharedApiUtils.Abstractons;
using WebSocketService.Clients;
using WebSocketService.Exceptions;
using WebSocketService.HubStates;
using WebSocketService.Interfaces;
using WebSocketService.Models;

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
            await Clients.Caller.CloseConnection(CloseConnectionReasons.InvalidJwtToken);
            hubState.BlockedClients.Add(Context.ConnectionId);
            return;
        }
        if (!string.IsNullOrEmpty(hubState.UserConnections.GetUserConnection(userId)))
        {
            await Clients.Caller.CloseConnection(CloseConnectionReasons.ConnectionAlreadyEstablished);
            hubState.BlockedClients.Add(Context.ConnectionId);
            return;
        }
        logger.LogInformation($"Client {Context.ConnectionId} has connected to SessionManagmentHub");
        hubState.UserConnections.AddOrSetUserConnectionId(userId, Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (hubState.BlockedClients.Contains(Context.ConnectionId))
        {
            hubState.BlockedClients.Remove(Context.ConnectionId);
            return;
        }   
        logger.LogInformation($"Client {Context.ConnectionId} has disconnected from SessionManagmentHub");
        string? userId = userContext.GetUserId(Context);
        if (string.IsNullOrEmpty(userId))
            return;
        hubState.UserConnections.RemoveUserConnection(userId, Context.ConnectionId);
    }
    public async Task<HubActionResult> JoinRoom(string roomId, string accessToken)
    {
        try
        {
            logger.LogInformation($"{Context.ConnectionId}: join room  request to room {roomId} with access token {accessToken}");
            if (hubState.BlockedClients.Contains(Context.ConnectionId))
                return new HubActionResult(false, ErrorMessages.ConnectionBlocked, "");
            string? userId = userContext.GetUserId(Context);
            if (userId is null)
                return new HubActionResult(false, ErrorMessages.PreferedUsernameClaimNotFound, null);
            
            try
            {
                string? userRoom = await serviceInternalRepository.GetUserRoom(userId);
                if (!string.IsNullOrEmpty(userRoom))
                    await roomSessionHandlerService.RemoveFromRoom(userId, userRoom);
            }
            catch(ErrorMessageException ex)
            {
                if(ex.Message == ErrorMessages.RoomIdNotExist || ex.Message == ErrorMessages.NotInRoom)
                    await serviceInternalRepository.DeleteUserRoom(userId);
            }
            await roomSessionHandlerService.AddToRoom(userId, roomId, accessToken);
            return new HubActionResult(true, null, null);
        }
        catch (ErrorMessageException ex)
        {
            return new HubActionResult(false, ex.Message, null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "An error occurred while joining the room.");
            return new HubActionResult(false, ErrorMessages.InternalServerError, null);
        }
    }
    public async Task<HubActionResult> GetRoomInformation(string roomId)
    {
        logger.LogInformation($"{Context.ConnectionId}: GetRoomInformation  request to room {roomId}");
        if (hubState.BlockedClients.Contains(Context.ConnectionId))
            return new HubActionResult(false, ErrorMessages.ConnectionBlocked, "");

        string? userId = userContext.GetUserId(Context);
        if (string.IsNullOrEmpty(userId))
            return new HubActionResult(false, ErrorMessages.PreferedUsernameClaimNotFound, null);

        try
        {
            var room = await roomSessionHandlerService.GetRoomInformation(roomId);
            HubActionResult reply = new HubActionResult(false, null, null);
            if (!room.Members.Contains(userId))
            {
                reply.ErrorMessage = ErrorMessages.NotAllowed;
                return reply;
            }
            reply.IsSuccess = true;
            reply.Payload = new
            {
                Members = new List<string>(room.Members),
                Room = new Models.RoomClientModel
                (room.RoomId,
                room.RoomName,
                room.Creator,
                room.SelectedPlayerCount,
                room.Members.Count)
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
        if (hubState.BlockedClients.Contains(Context.ConnectionId))
            return new HubActionResult(false, ErrorMessages.ConnectionBlocked, "");
        string? userId = userContext.GetUserId(Context);
        if (string.IsNullOrEmpty(userId))
            return new HubActionResult(false, ErrorMessages.PreferedUsernameClaimNotFound, null);

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
        if (hubState.BlockedClients.Contains(Context.ConnectionId))
            return new HubActionResult(false, ErrorMessages.ConnectionBlocked, "");
        string? userId = userContext.GetUserId(Context);
        if (string.IsNullOrEmpty(userId))
            return new HubActionResult(false, ErrorMessages.PreferedUsernameClaimNotFound, null);
        try
        {
            await roomSessionHandlerService.StartGame(roomId, userId);
            return new HubActionResult(true, null, null);
        }
        catch (ErrorMessageException ex)
        {
            return new HubActionResult(false, ex.Message, null);
        }
        catch (Exception)
        {
            return new HubActionResult(false, ErrorMessages.InternalServerError, null);
        }
    }
    public async Task<HubActionResult> GetSessionInformation(string sessionId)
    {
        try
        {
            logger.LogInformation($"GetSessionInformation request for session {sessionId}");
            if (hubState.BlockedClients.Contains(Context.ConnectionId))
                return new HubActionResult(false, ErrorMessages.ConnectionBlocked, "");
            string? userId = userContext.GetUserId(Context);
            if (string.IsNullOrEmpty(userId))
                return new HubActionResult(false, ErrorMessages.PreferedUsernameClaimNotFound, null);

            SessionInformation sessionInformation = await gameSessionHandlerService.GetSessionInformation(sessionId, userId);
            return new HubActionResult(true, null, sessionInformation);
        }
        catch (ErrorMessageException ex)
        {
            return new HubActionResult(false, ex.Message, null);
        }
        catch (Exception)
        {
            return new HubActionResult(false, ErrorMessages.InternalServerError, null);
        }
    }
    public async Task<HubActionResult> SyncGameState(string sessionId)
    {
        try
        {
            logger.LogInformation($"SyncGameState request for session {sessionId}");
            if (hubState.BlockedClients.Contains(Context.ConnectionId))
                return new HubActionResult(false, ErrorMessages.ConnectionBlocked, "");
            string? userId = userContext.GetUserId(Context);
            if (string.IsNullOrEmpty(userId))
                return new HubActionResult(false, ErrorMessages.PreferedUsernameClaimNotFound, null);
            string gameState = await gameSessionHandlerService.SyncGameState(sessionId, userId);
            return new HubActionResult(true, null, gameState);
        }
        catch (ErrorMessageException ex)
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
                return new HubActionResult(false, ErrorMessages.PreferedUsernameClaimNotFound, null);
            string? gameErrorMessage = await gameSessionHandlerService.MakeAction(userId, sessionId, action, payload);
            return new HubActionResult(true, null, gameErrorMessage);
        }
        catch (ErrorMessageException ex)
        {
            return new HubActionResult(false, ex.Message, null);
        }
        catch (Exception)
        {
            return new HubActionResult(false, ErrorMessages.InternalServerError, null);
        }

    }
}
