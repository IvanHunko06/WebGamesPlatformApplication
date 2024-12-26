using GameSessionService.Interface;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using SharedApiUtils;
using SharedApiUtils.Interfaces;
using SharedApiUtils.ServicesAccessing.Protos;
using System.Text.Json;

namespace GameSessionService.Services;

[Authorize(Policy = "OnlyPrivateClient")]
public class GameSessionService : GameSession.GameSessionBase
{
    private readonly IRoomsServiceClient roomsService;
    private readonly IGamesServiceClient gamesService;
    private readonly ILogger<GameSessionService> logger;
    private readonly ISessionsRepository sessionsRepository;
    private readonly IGameProcessingServiceClient gameProcessingService;

    public GameSessionService(IRoomsServiceClient roomsService,
        IGamesServiceClient gamesService,
        ILogger<GameSessionService> logger,
        ISessionsRepository sessionsRepository,
        IGameProcessingServiceClient gameProcessingService)
    {
        this.roomsService = roomsService;
        this.gamesService = gamesService;
        this.logger = logger;
        this.sessionsRepository = sessionsRepository;
        this.gameProcessingService = gameProcessingService;
    }
    public override async Task<StartGameSessionReply> StartGameSession(StartGameSessionRequest request, ServerCallContext context)
    {
        StartGameSessionReply reply = new StartGameSessionReply();
        logger.LogInformation($"Start new session for room {request.RoomId}");
        try
        {
            var sessionForRoom = (await sessionsRepository.GetSessionsList()).Where(s => s.RoomId == request.RoomId).FirstOrDefault();
            if (sessionForRoom is not null)
            {
                reply.ErrorMessage = ErrorMessages.SessionForRoomExist;
                return reply;
            }
            string sessionId = Guid.NewGuid().ToString();
            Models.GameSession gameSession = new Models.GameSession()
            {
                SessionId = sessionId,
                StartTime = DateTimeOffset.UtcNow,
                ActionsLog = new(),
                Players = new List<string>(request.Members),
                LastUpdated = DateTimeOffset.UtcNow,
                RoomId = request.RoomId,
                EndTime = null,
                SessionState = null,
            };
            gameSession.PlayerScores = new Dictionary<string, int>();
            request.Members.Select(member =>
            {
                gameSession.PlayerScores[member] = 0;
                return member;
            });
            GetRoomReply? getRoomReply = await roomsService.GetRoom(request.RoomId);
            if (getRoomReply is null)
            {
                reply.ErrorMessage = ErrorMessages.InternalServerError;
                return reply;
            }
            if (!getRoomReply.IsSuccess)
            {
                reply.ErrorMessage = getRoomReply.ErrorMessage;
                logger.LogInformation($"Room id not exist {request.RoomId}");
                return reply;
            }
            gameSession.OwnerId = getRoomReply.Room.Creator;
            gameSession.GameId = getRoomReply.Room.GameId;
            GameInfo? gameInfo = await gamesService.GetGameInfo(gameSession.GameId);
            if (gameInfo is null)
            {
                reply.ErrorMessage = ErrorMessages.GameIdNotValid;
                return reply;
            }
            gameSession.GameLogicServerUrl = gameInfo.GameLogicServerUrl;
            string? emptySessionState = await gameProcessingService.GetEmptySessionState(gameSession.GameLogicServerUrl, gameSession.Players);
            if (emptySessionState is null)
            {
                reply.ErrorMessage = ErrorMessages.InternalServerError;
                return reply;
            }
            gameSession.SessionState = emptySessionState;
            await sessionsRepository.AddOrUpdateSession(gameSession);
            reply.IsSuccess = true;
            reply.SessionId = sessionId;
            logger.LogInformation($"Session {sessionId} created for room {request.RoomId}");
            logger.LogDebug($"Session {sessionId}:\n{JsonSerializer.Serialize(gameSession)}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating a session");
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }

        return reply;
    }
    public override async Task<SendGameEventReply> SendGameEvent(SendGameEventRequest request, ServerCallContext context)
    {
        logger.LogInformation($"SendGameEvent request for session {request.SessionId}");
        SendGameEventReply reply = new SendGameEventReply();
        try
        {
            var gameSession = await sessionsRepository.GetSessionById(request.SessionId);
            if (gameSession is null)
            {
                reply.ErrorMessage = ErrorMessages.SessionIdNotExist;
                return reply;
            }
            logger.LogDebug($"SendGameEvent: game session {gameSession} found");
            if (!gameSession.Players.Contains(request.UserId))
            {
                reply.ErrorMessage = ErrorMessages.UserNotInSession;
                logger.LogInformation($"User {request.UserId} not in session {request.SessionId}");
                return reply;
            }
            ProccessActionReply? proccessActionReply = await gameProcessingService.ProccessAction(gameSession.GameLogicServerUrl, gameSession.SessionState,
                request.UserId, request.Action, request.Payload);
            if (proccessActionReply is null)
            {
                reply.ErrorMessage = ErrorMessages.InternalServerError;
                logger.LogInformation($"SendGameEvent: proccessActionReply is null");
                return reply;
            }
            if (!proccessActionReply.IsSuccess)
            {
                Models.NotifyMessage notifyCallerMessage = new Models.NotifyMessage()
                {
                    IsSuccess = false,
                    GameErrorMessage = proccessActionReply.GameErrorMessage
                };
                if (proccessActionReply.HasNotifyCaller)
                    notifyCallerMessage.Payload = proccessActionReply.NotifyCaller;
                reply.IsSuccess = true;
                reply.NotifyCallerMessage = JsonSerializer.Serialize(notifyCallerMessage);
                return reply;
            }
            if (proccessActionReply.HasNotifyRoom)
            {
                Models.NotifyMessage notifyRoomMessage = new Models.NotifyMessage()
                {
                    IsSuccess = true,
                    GameErrorMessage = null
                };
                notifyRoomMessage.Payload = proccessActionReply.NotifyRoom;
                reply.NotifyRoomMessage = JsonSerializer.Serialize(notifyRoomMessage);
            }
            if (proccessActionReply.HasNotifyCaller)
            {
                Models.NotifyMessage notifyCallerMessage = new Models.NotifyMessage()
                {
                    IsSuccess = true,
                    GameErrorMessage = null
                };
                notifyCallerMessage.Payload = proccessActionReply.NotifyCaller;
                reply.NotifyCallerMessage = JsonSerializer.Serialize(notifyCallerMessage);
            }
            gameSession.SessionState = proccessActionReply.NewSessionState;
            gameSession.LastUpdated = DateTimeOffset.UtcNow;
            gameSession.ActionsLog.Add(new Models.GameAction()
            {
                ActionType = request.Action,
                Payload = request.Payload,
                PlayerId = request.UserId,
                Timestamp = DateTimeOffset.UtcNow,
            });
            foreach (var player in proccessActionReply.PlayerScoreDelta)
            {
                if (gameSession.PlayerScores.ContainsKey(player.Key))
                {
                    gameSession.PlayerScores[player.Key] += player.Value;
                }
            }
            await sessionsRepository.AddOrUpdateSession(gameSession);
            reply.IsSuccess = true;
            logger.LogInformation($"SendGameEvent: game action has proccessed");
            logger.LogDebug($"SendGameEvent: reply: {JsonSerializer.Serialize(reply)}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing an action for the session.");
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }
        return reply;
    }
    public override async Task<EndGameSessionReply> EndGameSession(EndGameSessionRequest request, ServerCallContext context)
    {
        EndGameSessionReply reply = new EndGameSessionReply();
        logger.LogInformation($"EndGameSession request for session {request.SessionId} with reason {request.EndReason}");
        try
        {
            var session = await sessionsRepository.GetSessionById(request.SessionId);
            if (session is null)
            {
                reply.ErrorMessage = ErrorMessages.SessionIdNotExist;
                return reply;
            }
            await sessionsRepository.DeleteSessionById(request.SessionId);
            reply.IsSuccess = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while terminating the session");
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }
        return reply;
    }
    public override async Task<GetGameSessionReply> GetGameSession(GetGameSessionRequest request, ServerCallContext context)
    {
        GetGameSessionReply reply = new GetGameSessionReply();
        try
        {
            Models.GameSession? gameSession = await sessionsRepository.GetSessionById(request.SessionId);
            if (gameSession is null)
            {
                reply.ErrorMessage = ErrorMessages.SessionIdNotExist;
                return reply;
            }
            GameSessionRPCModel model = new GameSessionRPCModel()
            {
                EndTime = gameSession.EndTime.ToString(),
                StartTime = gameSession.StartTime.ToString(),
                GameId = gameSession.GameId,
                RoomId = gameSession.RoomId,
                OwnerId = gameSession.OwnerId,
                SessionId = gameSession.SessionId,
                LastUpdated = gameSession.LastUpdated.ToString(),
                SessionState = gameSession.SessionState,
            };
            model.Players.AddRange(gameSession.Players);
            model.PlayerScores.AddRange(gameSession.PlayerScores.Select(p =>
            {
                return new StringIntPair
                {
                    Key = p.Key,
                    Value = p.Value
                };
            }));
            model.ActionsLog.AddRange(gameSession.ActionsLog.Select(l =>
            {
                return new GameActionRPCModel
                {
                    ActionType = l.ActionType,
                    Payload = JsonSerializer.Serialize(l.Payload),
                    PlayerId = l.PlayerId,
                    Timestamp = l.Timestamp.ToString()
                };
            }));
            reply.IsSuccess = true;
            reply.GameSession = model;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the session");
        }


        return reply;
    }
}
