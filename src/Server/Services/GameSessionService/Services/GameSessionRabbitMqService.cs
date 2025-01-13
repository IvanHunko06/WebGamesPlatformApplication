using GameSessionService.Interfaces;
using SharedApiUtils.Abstractons;
using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Core.Messages.GameSessionService;
using SharedApiUtils.RabbitMq.Listeners;

namespace GameSessionService.Services;

public sealed class GameSessionRabbitMqService : BaseRabbitMqGameSessionListener
{
    private readonly IGameSessionService gameSessionService;

    public GameSessionRabbitMqService(
        IGameSessionService gameSessionService,
        RabbitMqConnection _connection,
        ILogger<BaseRabbitMqGameSessionListener> _logger1,
        ILogger<BaseRabbitMqMessageListener> _logger2) :base(_connection,_logger1, _logger2)
    {
        this.gameSessionService = gameSessionService;
    }
    protected override async Task<EndGameSessionReply> EndGameSession(EndGameSessionRequest request)
    {
        var reply = new EndGameSessionReply();
        string? errorMessage = await gameSessionService.EndGameSession(request.SessionId, request.Reason, request.Payload);
        if (string.IsNullOrEmpty(errorMessage))
            reply.IsSuccess = true;
        else
            reply.ErrorMessage = errorMessage;

        return reply;
    }

    protected override async Task<GetGameSessionReply> GetGameSession(GetGameSessionRequest request)
    {
        var reply = new GetGameSessionReply();
        var session = await gameSessionService.GetGameSession(request.SessionId);
        if(session is null)
        {
            reply.ErrorMessage = ErrorMessages.SeasonNotFound;
            return reply;
        }
        reply.IsSuccess = true;
        reply.GameSession = new SharedApiUtils.Abstractons.Core.GameSessionDto()
        {
            EndTime = session.EndTime,
            StartTime = session.StartTime,
            SessionId = session.SessionId,
            SessionState = session.SessionState,
            PlayerScores = session.PlayerScores,
            OwnerId = session.OwnerId,
            GameId = session.GameId,
            Players = session.Players,
            RoomId = session.RoomId,
        };
        return reply;
    }

    protected override async Task<SendGameEventReply> SendGameEvent(SendGameEventRequest request)
    {
        var sendEventResult = await gameSessionService.SendGameEvent(request.SessionId, request.PlayerId, request.Action, request.Payload);
        var reply = new SendGameEventReply();
        if (string.IsNullOrEmpty(sendEventResult.errorMessage) && string.IsNullOrEmpty(sendEventResult.gameErrorMessage))
            reply.IsSuccess = true;
        else if(!string.IsNullOrEmpty(sendEventResult.errorMessage))
            reply.ErrorMessage = sendEventResult.errorMessage;
        else if(!string.IsNullOrEmpty(sendEventResult.gameErrorMessage))
            reply.GameErrorMessage = sendEventResult.gameErrorMessage;

        return reply;

    }

    protected override async Task<StartGameSessionReply> StartGameSession(StartGameSessionRequest request)
    {
        var startResult = await gameSessionService.StartGameSession(request.RoomId);
        var reply = new StartGameSessionReply();
        if (!string.IsNullOrEmpty(startResult.errorMessage))
        {
            reply.ErrorMessage = startResult.errorMessage;
            return reply;
        }
        reply.IsSuccess = true;
        reply.SessionId = startResult.sessionId;
        return reply;
    }

    protected override async Task<SyncGameStateReply> SyncGameState(SyncGameStateRequest request)
    {
        var syncResult = await gameSessionService.SyncGameState(request.SessionId, request.PlayerId);
        var reply = new SyncGameStateReply();
        if(!string.IsNullOrEmpty(syncResult.errorMessage))
        {
            reply.ErrorMessage = syncResult.errorMessage;
            return reply;
        }
        reply.IsSuccess = true;
        reply.GameState = syncResult.sessionState;
        return reply;
    }
}
