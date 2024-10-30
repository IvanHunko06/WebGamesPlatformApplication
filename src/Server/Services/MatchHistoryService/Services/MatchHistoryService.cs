using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MatchHistoryService.Protos;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using MatchHistoryService.Models;
using Microsoft.AspNetCore.Authorization;
namespace MatchHistoryService.Services;

public class MatchHistoryService : Protos.MatchesHistory.MatchesHistoryBase
{
    private readonly ILogger<MatchHistoryService> logger;
    private readonly MatchHistoryDbContext dbContext;

    public MatchHistoryService(ILogger<MatchHistoryService> logger, MatchHistoryDbContext dbContext)
    {
        this.logger = logger;
        this.dbContext = dbContext;
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<GetMatchesInfoReply> GetMatchesInfo(Empty request, ServerCallContext context)
    {
        var reply = new GetMatchesInfoReply();

        try
        {
            var matches = await dbContext.MatchInformations.Include(m => m.MatchMembers).ToListAsync();
            reply.Matches.AddRange(matches.Select(match => new MatchInfo
            {
                TimeBegin = match.TimeBegin.ToString("o"),
                TimeEnd = match.TimeEnd.ToString("o"),
                FinishReason = match.FinishReason,
                GameId = match.GameId,
                MatchMembers = { match.MatchMembers.Select(member => new PlayerScore
                    {
                        UserId = member.UserId,
                        Score = member.ScorePoints,
                        IsWinner = member.IsWinner
                    })}
            }));
        }
        catch (Exception ex)
        {
            logger.LogError($"Error retrieving matches info: {ex}");
        }

        return reply;
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<GetMatchesInfoForPlayerReply> GetMatchesInfoForPlayer(GetMatchesInfoForPlayerRequest request, ServerCallContext context)
    {
        var reply = new GetMatchesInfoForPlayerReply();

        try
        {
            var matchesQuery = dbContext.MatchInformations
                .Include(m => m.MatchMembers)
                .Where(m => m.MatchMembers.Any(p => p.UserId == request.UserId));

            var matches = await matchesQuery.ToListAsync();

            if (matches.Count == 0)
            {
                reply.ErrorMessage = ErrorMessages.NotFound;
                return reply;
            }

            reply.Matches.AddRange(matches.Select(match => new PlayerMatchInfo
            {
                TimeBegin = match.TimeBegin.ToString("o"),
                TimeEnd = match.TimeEnd.ToString("o"),
                FinishReason = match.FinishReason,
                GameId = match.GameId,
                MatchMembers = { match.MatchMembers.Select(member => new PlayerScore
                    {
                        UserId = member.UserId,
                        Score = member.ScorePoints,
                        IsWinner = member.IsWinner
                    })}
            }));
        }
        catch (Exception ex)
        {
            logger.LogError($"Error retrieving matches info for player {request.UserId}: {ex}");
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }

        return reply;
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<AddMatchInfoReply> AddMatchInfo(MatchInfo request, ServerCallContext context)
    {
        var reply = new AddMatchInfoReply();


        try
        {
            var match = new MatchInformation
            {
                TimeBegin = DateTimeOffset.Parse(request.TimeBegin),
                TimeEnd = DateTimeOffset.Parse(request.TimeEnd),
                FinishReason = request.FinishReason,
                GameId = request.GameId,
                MatchMembers = request.MatchMembers.Select(m => new Score
                {
                    UserId = m.UserId,
                    ScorePoints = m.Score,
                    IsWinner = m.IsWinner
                }).ToList()
            };

            dbContext.MatchInformations.Add(match);
            await dbContext.SaveChangesAsync();

            reply.IsSuccess = true;
        }
        catch (FormatException ex)
        {
            logger.LogError($"Invalid date format in AddMatchInfo: {ex}");
            reply.IsSuccess = false;
            reply.ErrorMessage = "Invalid date format";
        }
        catch (DbUpdateException ex)
        {
            logger.LogError($"Database update error in AddMatchInfo: {ex}");
            reply.IsSuccess = false;
            reply.ErrorMessage = "Database update error";
        }
        catch (Exception ex)
        {
            logger.LogError($"Unexpected error in AddMatchInfo: {ex}");
            reply.IsSuccess = false;
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }

        return reply;
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<DeleteMatchInfoReply> DeleteMatchInfo(DeleteMatchInfoRequest request, ServerCallContext context)
    {
        var reply = new DeleteMatchInfoReply();


        try
        {
            var match = await dbContext.MatchInformations
                .Include(m => m.MatchMembers)
                .FirstOrDefaultAsync(m => m.Id == request.MatchId);

            if (match == null)
            {
                reply.IsSuccess = false;
                reply.ErrorMessage = "Match not found";
                return reply;
            }
            dbContext.PlayerScores.RemoveRange(match.MatchMembers);
            dbContext.MatchInformations.Remove(match);
            await dbContext.SaveChangesAsync();

            reply.IsSuccess = true;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError($"Database update error in DeleteMatchInfo: {ex}");
            reply.IsSuccess = false;
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }
        catch (Exception ex)
        {
            logger.LogError($"Unexpected error in DeleteMatchInfo: {ex}");
            reply.IsSuccess = false;
            reply.ErrorMessage = ErrorMessages.InternalServerError;
        }

        return reply;
    }
}
