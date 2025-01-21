using System.Text.Json;
using GameSessionService.Interfaces;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Interfaces.Clients;

namespace GameSessionService.Services;

public class GameSessionService : IGameSessionService
{
    private readonly ILogger<GameSessionService> logger;
    private readonly ISessionsRepository sessionsRepository;
    private readonly IRoomsServiceClient roomsService;
    private readonly IGameProcessingServiceClient gameProcessingService;
    private readonly IGameSessionWsNotifyerClient gameSessionWsNotifyer;
    private readonly IRatingServiceClient ratingService;
    private readonly IMatchHistoryServiceClient matchHistoryService;

    public GameSessionService(
        ILogger<GameSessionService> logger,
        ISessionsRepository sessionsRepository,
        IRoomsServiceClient roomsService,
        IGameProcessingServiceClient gameProcessingService,
        IGameSessionWsNotifyerClient gameSessionWsNotifyer,
        IRatingServiceClient ratingService,
        IMatchHistoryServiceClient matchHistoryService)
    {
        this.logger = logger;
        this.sessionsRepository = sessionsRepository;
        this.roomsService = roomsService;
        this.gameProcessingService = gameProcessingService;
        this.gameSessionWsNotifyer = gameSessionWsNotifyer;
        this.ratingService = ratingService;
        this.matchHistoryService = matchHistoryService;
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
            logger.LogDebug($"Getting room information. RoomId: {roomId}");
            var getRoomReply = await roomsService.GetRoom(roomId);
            if (!string.IsNullOrEmpty(getRoomReply.errorMessage))
            {
                logger.LogInformation($"Getting room {roomId} error. ErrorMessage: {getRoomReply.errorMessage}");
                return (getRoomReply.errorMessage, null);
            }
            logger.LogDebug($"Room {roomId} info recived");
            gameSession.OwnerId = getRoomReply.roomModel.Creator;
            gameSession.GameId = getRoomReply.roomModel.GameId;
            gameSession.PlayerScores = new Dictionary<string, int>();
            gameSession.Players = new List<string>();
            foreach (var member in getRoomReply.roomModel.Members)
            {
                gameSession.Players.Add(member);
                gameSession.PlayerScores[member] = 0;
            }
            logger.LogDebug($"Getting empty session state. GameId: {gameSession.GameId}");
            string? emptySessionState = await gameProcessingService.GetEmptySessionState(gameSession.GameId, gameSession.Players);
            if (emptySessionState == ErrorMessages.InternalServerError)
                return (ErrorMessages.InternalServerError, null);
            else if (emptySessionState == ErrorMessages.IncorrectGamePlayersCount)
                return (ErrorMessages.IncorrectGamePlayersCount, null);
            logger.LogDebug($"Empty session state recived");
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
            logger.LogInformation($"Send game event for session: {sessionId}. Action: {action}. Payload: {payload}");
            var gameSession = await sessionsRepository.GetSessionById(sessionId);
            if (gameSession is null)
                return (ErrorMessages.SessionIdNotExist, null);


            logger.LogDebug($"SendGameEvent: game session {gameSession.SessionId} found");
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
            string oldSessionState = gameSession.SessionState;
            var proccessResult = await gameProcessingService.ProccessAction(gameSession.GameId, gameSession.SessionState,
                userId, action, payload);
            if (!string.IsNullOrEmpty(proccessResult.gameErrorMessage))
                return (null, proccessResult.gameErrorMessage);

            gameSession.SessionState = proccessResult.newSessionState;
            string newSessionState = proccessResult.newSessionState;
            await sessionsRepository.AddOrUpdateSession(gameSession);
            logger.LogInformation($"Session {gameSession.SessionId} updated");
            _ = Task.Run(async () =>
            {
                try
                {
                    var notifyChanges = await gameProcessingService.GetSessionDeltaMessages(gameSession.GameId, oldSessionState, newSessionState);
                    if (!string.IsNullOrEmpty(notifyChanges.notifyRoomMessage))
                    {
                        logger.LogInformation($"Notifying session {gameSession.SessionId} with message {notifyChanges.notifyRoomMessage}");
                        _ = gameSessionWsNotifyer.NotifyReciveAction_AllUsers(gameSession.SessionId, notifyChanges.notifyRoomMessage);
                    }


                    if (notifyChanges.notifyPlayers is not null)
                    {
                        foreach (var notifyMessage in notifyChanges.notifyPlayers)
                        {
                            logger.LogInformation($"Notifying session {gameSession.SessionId} player {notifyMessage.Key} with message {notifyMessage.Value}");
                            _ = gameSessionWsNotifyer.NotifyReciveAction_User(gameSession.SessionId, notifyMessage.Key, notifyMessage.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occurred while sending game state change messages");
                }
            });

            _ = Task.Run(async () =>
            {
                try
                {
                    logger.LogInformation($"Checking game over status for session {gameSession.SessionId}");
                    var checkWinResult = await gameProcessingService.CheckGameOver(gameSession.GameId, gameSession.SessionState);
                    if (checkWinResult.IsOver == false)
                    {
                        logger.LogInformation($"Session {gameSession.SessionId} in progress");
                        return;
                    }
                    logger.LogInformation($"Session {gameSession.SessionId} is over.");

                    await EndGameSession(gameSession.SessionId, EndSessionReason.NormalFinish, JsonSerializer.Serialize(checkWinResult.PlayerScores));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred in the logic for handling the game end check.");
                }
            });
            return (null, null);
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
            session.EndTime = DateTimeOffset.UtcNow;
            if (reason == EndSessionReason.NormalFinish)
                await NormalSessionFinish(payload ?? "", session);
            else if (reason == EndSessionReason.PlayerDisconnected)
                await PlayerDisconnectedFinish(payload ?? "", session);
            else
                await DefaultSessionFinish(sessionId, reason, payload);

            _ = Task.Run(async () =>
            {
                string? errorMessage = await matchHistoryService.AddMatchInfo(
                    session.GameId, 
                    reason,
                    session.StartTime, 
                    session.EndTime.Value,
                    session.PlayerScores);
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    logger.LogWarning($"An error occurred while saving the match history. Error message: {errorMessage}");
            });
            
            await sessionsRepository.DeleteSessionById(sessionId);
            string? deleteRoomErrorMessage = await roomsService.DeleteRoom(session.RoomId);
            if (!string.IsNullOrEmpty(deleteRoomErrorMessage))
                logger.LogWarning($"Room {session.RoomId} deleting error {deleteRoomErrorMessage}");
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
        try
        {
            var session = await sessionsRepository.GetSessionById(sessionId);
            if (session is null)
                return (ErrorMessages.SessionIdNotExist, null);
            string gameState = await gameProcessingService.GetGameStateForPlayer(session.GameId, userId, session.SessionState);
            if (gameState == ErrorMessages.InternalServerError)
                return (ErrorMessages.InternalServerError, null);

            return (null, gameState);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while getting the game state for the player {userId}");
            return (ErrorMessages.InternalServerError, null);
        }
    }

    private async Task NormalSessionFinish(string jsonPayload, Models.GameSessionModel gameSession)
    {
        try
        {
            Dictionary<string, int>? playerScores = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonPayload);
            if (playerScores is null) return;

            logger.LogInformation($"Notifying session players");
            foreach (var score in playerScores)
            {
                logger.LogDebug($"Notifying user {score.Key} with score {score.Value}");
                gameSession.PlayerScores[score.Key] = score.Value;
                await gameSessionWsNotifyer.NotifySessionEnded_User(gameSession.SessionId, score.Key, EndSessionReason.NormalFinish, score.Value.ToString());
                _ = Task.Run(async () =>
                {
                    string? errorMessage = await ratingService.AddLastSeasonUserScore(score.Key, score.Value);
                    if (!string.IsNullOrEmpty(errorMessage))
                        logger.LogWarning($"an error occurred while updating the user rating. Error message: {errorMessage}");
                });
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing a normal session termination");
        }
    }
    private async Task PlayerDisconnectedFinish(string disconnectedPlayer, Models.GameSessionModel gameSession)
    {
        if (string.IsNullOrEmpty(disconnectedPlayer)) return;
        gameSession.PlayerScores[disconnectedPlayer] = (gameSession.Players.Count - 1) * 2 * -1;
        foreach (var player in gameSession.Players)
        {
            if(player == disconnectedPlayer) continue;
            gameSession.PlayerScores[player] = 2;
            _ = Task.Run(async () =>
            {
                string? errorMessage = await ratingService.AddLastSeasonUserScore(player, gameSession.PlayerScores[player]);
                if (!string.IsNullOrEmpty(errorMessage))
                    logger.LogWarning($"an error occurred while updating the user rating. Error message: {errorMessage}");
            });
        }
        await gameSessionWsNotifyer.NotifySessionEnded_AllUser(gameSession.SessionId, EndSessionReason.PlayerDisconnected, disconnectedPlayer); 
    }
    private async Task DefaultSessionFinish(string sessionId, string reason, string? payload)
    {
        await gameSessionWsNotifyer.NotifySessionEnded_AllUser(sessionId, reason, payload);
    }
}
