using GameSessionService.Interfaces;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;

namespace GameSessionService.Services;

[Authorize(Policy = "OnlyPrivateClient")]
public class GameSessionRpcService : GameSession.GameSessionBase
{
    private readonly IGameSessionService gameSessionService;

    public GameSessionRpcService(IGameSessionService gameSessionService)
    {
        this.gameSessionService = gameSessionService;
    }
    public override async Task<StartGameSessionReply> StartGameSession(StartGameSessionRequest request, ServerCallContext context)
    {
        StartGameSessionReply reply = new StartGameSessionReply();
        var startResult = await gameSessionService.StartGameSession(request.RoomId);
        if (!string.IsNullOrEmpty(startResult.errorMessage))
        {
            reply.ErrorMessage = startResult.errorMessage;
            return reply;
        }
        reply.IsSuccess = true;
        reply.SessionId = startResult.sessionId;
        return reply;
    }
    public override async Task<SendGameEventReply> SendGameEvent(SendGameEventRequest request, ServerCallContext context)
    {

        SendGameEventReply reply = new SendGameEventReply();
        var sendReply = await gameSessionService.SendGameEvent(request.SessionId, request.UserId, request.Action, request.Payload);
        if (string.IsNullOrEmpty(sendReply.errorMessage) && string.IsNullOrEmpty(sendReply.gameErrorMessage))
            reply.IsSuccess = true;
        else if (!string.IsNullOrEmpty(sendReply.errorMessage))
            reply.ErrorMessage = sendReply.errorMessage;
        else if (!string.IsNullOrEmpty(sendReply.gameErrorMessage))
            reply.GameErrorMessage = sendReply.gameErrorMessage;

        return reply;
    }
    public override async Task<EndGameSessionReply> EndGameSession(EndGameSessionRequest request, ServerCallContext context)
    {
        EndGameSessionReply reply = new EndGameSessionReply();
        string? errorMessage = await gameSessionService.EndGameSession(request.SessionId, request.EndReason, request.Payload);
        if (string.IsNullOrEmpty(errorMessage))
            reply.IsSuccess = true;
        else
            reply.ErrorMessage = errorMessage;

        return reply;
    }
    public override async Task<GetGameSessionReply> GetGameSession(GetGameSessionRequest request, ServerCallContext context)
    {
        GetGameSessionReply reply = new GetGameSessionReply();
        //try
        //{
        //    Models.GameSessionModel? gameSession = await sessionsRepository.GetSessionById(request.SessionId);
        //    if (gameSession is null)
        //    {
        //        reply.ErrorMessage = ErrorMessages.SessionIdNotExist;
        //        return reply;
        //    }
        //    GameSessionRPCModel model = new GameSessionRPCModel()
        //    {
        //        EndTime = gameSession.EndTime.ToString(),
        //        StartTime = gameSession.StartTime.ToString(),
        //        GameId = gameSession.GameId,
        //        RoomId = gameSession.RoomId,
        //        OwnerId = gameSession.OwnerId,
        //        SessionId = gameSession.SessionId,
        //        LastUpdated = gameSession.LastUpdated.ToString(),
        //        SessionState = gameSession.SessionState,
        //    };
        //    model.Players.AddRange(gameSession.Players);
        //    model.PlayerScores.AddRange(gameSession.PlayerScores.Select(p =>
        //    {
        //        return new StringIntPair
        //        {
        //            Key = p.Key,
        //            Value = p.Value
        //        };
        //    }));
        //    model.ActionsLog.AddRange(gameSession.ActionsLog.Select(l =>
        //    {
        //        return new GameActionRPCModel
        //        {
        //            ActionType = l.ActionType,
        //            Payload = JsonSerializer.Serialize(l.Payload),
        //            PlayerId = l.PlayerId,
        //            Timestamp = l.Timestamp.ToString()
        //        };
        //    }));
        //    reply.IsSuccess = true;
        //    reply.GameSession = model;
        //}
        //catch (Exception ex)
        //{
        //    logger.LogError(ex, "An error occurred while retrieving the session");
        //}


        return reply;
    }

}
