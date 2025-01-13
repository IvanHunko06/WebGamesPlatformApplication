using GameSessionService.Interfaces;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using System.Text.Json;

namespace GameSessionService.Services;

public class GameSessionService : IGameSessionService
{
    private readonly ILogger<GameSessionService> logger;
    private readonly ISessionsRepository sessionsRepository;
    private readonly IRoomsServiceClient roomsService;
    private readonly IGameProcessingServiceClient gameProcessingService;

    public GameSessionService(
        ILogger<GameSessionService> logger,
        ISessionsRepository sessionsRepository,
        IRoomsServiceClient roomsService,
        IGameProcessingServiceClient gameProcessingService)
    {
        this.logger = logger;
        this.sessionsRepository = sessionsRepository;
        this.roomsService = roomsService;
        this.gameProcessingService = gameProcessingService;
    }
    public async Task<(string? errorMessage, string? sessionId)> StartGameSession(string roomId)
    {
        logger.LogInformation($"Start new session for room {roomId}");
        try
        {
            var sessionForRoom = (await sessionsRepository.GetSessionsList()).Where(s => s.RoomId == roomId).FirstOrDefault();
            if (sessionForRoom is not null)
                return (ErrorMessages.SessionForRoomExist, null);

            string sessionId = Guid.NewGuid().ToString();
            Models.GameSessionModel gameSession = new Models.GameSessionModel()
            {
                SessionId = sessionId,
                StartTime = DateTimeOffset.UtcNow,
                ActionsLog = new(),
                LastUpdated = DateTimeOffset.UtcNow,
                RoomId = roomId,
                EndTime = null,
                SessionState = "",
            };

            var getRoomReply = await roomsService.GetRoom(roomId);
            if (!string.IsNullOrEmpty(getRoomReply.errorMessage))
            {
                logger.LogInformation($"Getting room {roomId} error. ErrorMessage: {getRoomReply.errorMessage}");
                return (getRoomReply.errorMessage, null);
            }
            gameSession.OwnerId = getRoomReply.roomModel.Creator;
            gameSession.GameId = getRoomReply.roomModel.GameId;
            gameSession.PlayerScores = new Dictionary<string, int>();
            gameSession.Players = new List<string>();
            foreach (var member in gameSession.Players)
            {
                gameSession.Players.Add(member);
                gameSession.PlayerScores[member] = 0;
            }
            string? emptySessionState = await gameProcessingService.GetEmptySessionState(gameSession.GameId, gameSession.Players);
            if (emptySessionState is null)
                return (ErrorMessages.InternalServerError, null);

            gameSession.SessionState = emptySessionState;
            await sessionsRepository.AddOrUpdateSession(gameSession);
            logger.LogInformation($"Session {sessionId} created for room {roomId}");
            logger.LogDebug($"Session {sessionId}:\n{JsonSerializer.Serialize(gameSession)}");
            return (null, sessionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating a session");
            return (ErrorMessages.InternalServerError, null);
        }
    }
    public async Task<(string? errorMessage, string? gameErrorMessage)> SendGameEvent(string sessionId, string userId, string action, string payload)
    {
        try
        {
            var gameSession = await sessionsRepository.GetSessionById(sessionId);
            if (gameSession is null)
                return (ErrorMessages.SessionIdNotExist, null);


            logger.LogDebug($"SendGameEvent: game session {gameSession} found");
            if (!gameSession.Players.Contains(userId))
            {
                logger.LogInformation($"User {userId} not in session {sessionId}");
                return (ErrorMessages.UserNotInSession, null);
            }
            gameSession.ActionsLog.Add(new Models.GameActionModel()
            {
                ActionType = action,
                Payload = payload,
                Timestamp = DateTimeOffset.UtcNow,
                PlayerId = userId,

            });
            gameSession.LastUpdated = DateTimeOffset.UtcNow;
            var proccessResult = await gameProcessingService.ProccessAction(gameSession.GameId, gameSession.SessionState,
                userId, action, payload);
            if (!string.IsNullOrEmpty(proccessResult.gameErrorMessage))
                return (null, proccessResult.gameErrorMessage);

            gameSession.SessionState = proccessResult.newSessionState;
            
            

            return (null, null);
            //if (!proccessActionReply.IsSuccess)
            //{
            //    Models.NotifyMessage notifyCallerMessage = new Models.NotifyMessage()
            //    {
            //        IsSuccess = false,
            //        GameErrorMessage = proccessActionReply.GameErrorMessage
            //    };
            //    if (proccessActionReply.HasNotifyCaller)
            //        notifyCallerMessage.Payload = proccessActionReply.NotifyCaller;
            //    reply.IsSuccess = true;
            //    reply.NotifyCallerMessage = JsonSerializer.Serialize(notifyCallerMessage);
            //    return reply;
            //}
            //if (proccessActionReply.HasNotifyRoom)
            //{
            //    Models.NotifyMessage notifyRoomMessage = new Models.NotifyMessage()
            //    {
            //        IsSuccess = true,
            //        GameErrorMessage = null
            //    };
            //    notifyRoomMessage.Payload = proccessActionReply.NotifyRoom;
            //    reply.NotifyRoomMessage = JsonSerializer.Serialize(notifyRoomMessage);
            //}
            //if (proccessActionReply.HasNotifyCaller)
            //{
            //    Models.NotifyMessage notifyCallerMessage = new Models.NotifyMessage()
            //    {
            //        IsSuccess = true,
            //        GameErrorMessage = null
            //    };
            //    notifyCallerMessage.Payload = proccessActionReply.NotifyCaller;
            //    reply.NotifyCallerMessage = JsonSerializer.Serialize(notifyCallerMessage);
            //}
            //gameSession.SessionState = proccessActionReply.NewSessionState;
            //gameSession.LastUpdated = DateTimeOffset.UtcNow;
            //gameSession.ActionsLog.Add(new Models.GameAction()
            //{
            //    ActionType = request.Action,
            //    Payload = request.Payload,
            //    PlayerId = request.UserId,
            //    Timestamp = DateTimeOffset.UtcNow,
            //});
            //foreach (var player in proccessActionReply.PlayerScoreDelta)
            //{
            //    if (gameSession.PlayerScores.ContainsKey(player.Key))
            //    {
            //        gameSession.PlayerScores[player.Key] += player.Value;
            //    }
            //}
            //await sessionsRepository.AddOrUpdateSession(gameSession);
            //reply.IsSuccess = true;
            //logger.LogInformation($"SendGameEvent: game action has proccessed");
            //logger.LogDebug($"SendGameEvent: reply: {JsonSerializer.Serialize(reply)}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing an action for the session.");
            return (ErrorMessages.InternalServerError, null);
        }
    }

    public async Task<Models.GameSessionModel?> GetGameSession(string sessionId)
    {
        try
        {
            var sesion = await sessionsRepository.GetSessionById(sessionId);
            return sesion;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while retrieving the session");
            return null;
        }
    }
    public async Task<string?> EndGameSession(string sessionId, string reason, string? payload)
    {
        logger.LogInformation($"EndGameSession request for session {sessionId} with reason {reason}");
        try
        {
            var session = await sessionsRepository.GetSessionById(sessionId);
            if (session is null)
                return ErrorMessages.SessionIdNotExist;
            await sessionsRepository.DeleteSessionById(sessionId);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while terminating the session");
            return ErrorMessages.InternalServerError;
        }
    }
    public async Task<(string? errorMessage, string? sessionState)> SyncGameState(string sessionId, string userId)
    {
        return (null, null);
    }
}
