using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MatchHistoryService.Interfaces;
using MatchHistoryService.Models;
using Microsoft.AspNetCore.Authorization;
using SharedApiUtils.Abstractons;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;
namespace MatchHistoryService.Services;

[Authorize(Policy = "AdminOrPrivateClient")]
public class MatchHistoryRpcService : MatchesHistory.MatchesHistoryBase
{
    private readonly IMatchHistoryService matchHistoryService;

    public MatchHistoryRpcService(IMatchHistoryService matchHistoryService)
    {
        this.matchHistoryService = matchHistoryService;
    }

    public override async Task<GetMatchesInfoReply> GetMatchesInfo(Empty request, ServerCallContext context)
    {
        var reply = new GetMatchesInfoReply();

        var matchInfos = await matchHistoryService.GetAllRecords();
        if (matchInfos is null)
            return reply;

        var replyMatchInfos = matchInfos.Select(x =>
        {
            var matchMembers = x.UserScoreDelta.Select(x=>new PlayerScore()
            {
                ScoreDelta = x.Value,
                UserId = x.Key
            }).ToList();
            var matchInfo = new MatchInfo()
            {
                FinishReason = x.FinishReason,
                GameId = x.GameId,
                RecordId = x.RecordId.ToString(),
                TimeBegin = x.TimeBegin.ToString(),
                TimeEnd = x.TimeEnd.ToString(),
            };
            matchInfo.MatchMembers.AddRange(matchMembers);
            return matchInfo;
        }).ToList();
        reply.Matches.AddRange(replyMatchInfos);
        return reply;
    }

    public override async Task<GetMatchesInfoForPlayerReply> GetMatchesInfoForPlayer(GetMatchesInfoForPlayerRequest request, ServerCallContext context)
    {
        var reply = new GetMatchesInfoForPlayerReply();

        var matchesForUser = await matchHistoryService.GetAllMatchesForUser(request.UserId);
        if (matchesForUser is null)
        {
            reply.ErrorMessage = ErrorMessages.InternalServerError;
            return reply;
        }
        if (matchesForUser.Count == 0)
        {
            reply.ErrorMessage = ErrorMessages.NotFound;
            return reply;
        }
        
        var matches = matchesForUser.Select(m =>
        {
            return new PlayerMatchInfo()
            {
                GameId = m.GameId,
                ScoreDelta = m.UserScoreDelta
                    .Where(x=>x.Key == request.UserId)
                    .Select(x=>x.Value)
                    .First(),
                TimeBegin = m.TimeBegin.ToString(),
                TimeEnd = m.TimeEnd.ToString(),
            };
        }).ToList();
        reply.Matches.AddRange(matches);
        reply.IsSuccess = true;
        return reply;
    }

    public override async Task<AddMatchInfoReply> AddMatchInfo(MatchInfo request, ServerCallContext context)
    {
        var reply = new AddMatchInfoReply();
        MatchInfoModel matchInfo = new MatchInfoModel()
        {
            FinishReason = request.FinishReason,
            GameId = request.GameId,
            RecordId = Guid.NewGuid(),
            TimeBegin = DateTimeOffset.Parse(request.TimeBegin),
            TimeEnd = DateTimeOffset.Parse(request.TimeEnd),
            UserScoreDelta = new Dictionary<string, int>()
        };
        foreach (var item in request.MatchMembers)
        {
            matchInfo.UserScoreDelta[item.UserId] = item.ScoreDelta;
        }
        string? errorMessage = await matchHistoryService.AddMatchInfo(matchInfo);
        if (string.IsNullOrEmpty(errorMessage))
            reply.IsSuccess = true;
        else
            reply.ErrorMessage = errorMessage;
        return reply;
    }

    public override async Task<DeleteMatchInfoReply> DeleteMatchInfo(DeleteMatchInfoRequest request, ServerCallContext context)
    {
        var reply = new DeleteMatchInfoReply();

        string? errorMessage = await matchHistoryService.DeleteMatchInfo(request.RecordId);
        if (string.IsNullOrEmpty(errorMessage))
            reply.IsSuccess = true;
        else
            reply.ErrorMessage = errorMessage;
        return reply;
    }
}
