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
using WebSocketService.Services;

namespace WebSocketService.Hubs;

[Authorize(Policy = "OnlyPublicClient")]
public class SessionManagmentHub : Hub<ISessionManagmentClient>
{
    private readonly ILogger<SessionManagmentHub> logger;
    private readonly SessionManagmentHubState hubState;
    private readonly RoomSessionHandlerService roomSessionHandlerService;

    public SessionManagmentHub(ILogger<SessionManagmentHub> logger, SessionManagmentHubState hubState, RoomSessionHandlerService roomSessionHandlerService)
    {
        this.logger = logger;
        this.hubState = hubState;
        this.roomSessionHandlerService = roomSessionHandlerService;
    }


    public override async Task OnConnectedAsync()
    {
        if (Context.User is null)
        {
            Context.Abort();
            return;
        }
        var subjectClaim = Context.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
        if (subjectClaim is null)
        {
            Context.Abort();
            return;
        }
        if (!string.IsNullOrEmpty(hubState.UserConnections.GetUserConnection(subjectClaim.Value)))
        {
            Context.Abort();
            return;
        }
        logger.LogInformation($"Client {Context.ConnectionId} has connected to SessionManagmentHub");
        hubState.UserConnections.AddOrSetUserConnectionId(subjectClaim.Value, Context.ConnectionId);
        hubState.ConnectedClients.Add(Context.ConnectionId);


    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (!hubState.ConnectedClients.Contains(Context.ConnectionId))
            return;
        logger.LogInformation($"Client {Context.ConnectionId} has disconnected from SessionManagmentHub");
        if (Context.User is null) return;
        var subjectClaim = Context.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
        if (subjectClaim is null) return;
        hubState.ConnectedClients.Remove(Context.ConnectionId);
        hubState.UserConnections.RemoveUserConnection(subjectClaim.Value, Context.ConnectionId);
    }
    public async Task<HubActionResult> JoinRoom(string roomId, string accessToken)
    {

        logger.LogInformation($"{Context.ConnectionId}: join room  request to room {roomId} with access token {accessToken}");
        if (Context.User is null)
            return new HubActionResult(false, ErrorMessages.SubjectClaimNotFound);

        var subjectClaim = Context.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
        if (subjectClaim is null)
            return new HubActionResult(false, ErrorMessages.SubjectClaimNotFound);
        if (!string.IsNullOrEmpty(roomSessionHandlerService.GetUserRoom(subjectClaim.Value)))
            return new HubActionResult(false, ErrorMessages.UserAsignedToRoom);

        try
        {
            await roomSessionHandlerService.AddToRoom(subjectClaim.Value, roomId, accessToken);
            return new HubActionResult(true, null);
        }
        catch(ErrorMessageException ex)
        {
            return new HubActionResult(false, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.ToString());
            return new HubActionResult(false, ErrorMessages.InternalServerError);
        }
    }
    public async Task<GetRoomInformationReply> GetRoomInformation(string roomId)
    {
        logger.LogInformation($"{Context.ConnectionId}: GetRoomInformation  request to room {roomId}");
        if (Context.User is null)
        {
            return new GetRoomInformationReply()
            {
                IsSuccess = false,
                ErrorMessage = ErrorMessages.SubjectClaimNotFound
            };
        }
        var subjectClaim = Context.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
        if (subjectClaim is null)
        {
            return new GetRoomInformationReply()
            {
                IsSuccess = false,
                ErrorMessage = ErrorMessages.SubjectClaimNotFound
            };
        }
        try
        {
            var getRoomReply = await roomSessionHandlerService.GetRoomInformation(roomId);
            GetRoomInformationReply reply = new GetRoomInformationReply();
            if (!getRoomReply.IsSuccess)
            {
                reply.ErrorMessage = getRoomReply.ErrorMessage;
                return reply;
            }
            if (!getRoomReply.Members.Contains(subjectClaim.Value))
            {
                return new GetRoomInformationReply()
                {
                    IsSuccess = false,
                    ErrorMessage = ErrorMessages.NotAllowed
                };
            }
            roomSessionHandlerService.SetUserRoom(subjectClaim.Value, roomId);
            reply.IsSuccess = true;
            reply.Members = [.. getRoomReply.Members];
            reply.Room = new Models.RoomModel
                (getRoomReply.Room.RoomId,
                getRoomReply.Room.RoomName,
                getRoomReply.Room.Creator,
                getRoomReply.Room.SelectedPlayersCount,
                reply.Members.Count);
            return reply;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex.ToString());
            return new GetRoomInformationReply()
            {
                ErrorMessage = ErrorMessages.InternalServerError,
                IsSuccess = false,
            };
        }
        

    }
    public async Task<HubActionResult> LeaveRoom(string roomId)
    {
        logger.LogInformation($"{Context.ConnectionId}: leave room  request from room {roomId}");
        if (Context.User is null)
            return new HubActionResult(false, ErrorMessages.SubjectClaimNotFound);

        var subjectClaim = Context.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
        if (subjectClaim is null)
            return new HubActionResult(false, ErrorMessages.SubjectClaimNotFound);


        try
        {
            await roomSessionHandlerService.RemoveFromRoom(subjectClaim.Value, roomId);
            return new HubActionResult(true, null);
        }
        catch(ErrorMessageException ex)
        {
            return new HubActionResult(false, ex.Message);
        }
        catch(Exception ex)
        {
            logger.LogWarning(ex.ToString());
            return new HubActionResult(false, ErrorMessages.InternalServerError);
        }
        
    }
}
