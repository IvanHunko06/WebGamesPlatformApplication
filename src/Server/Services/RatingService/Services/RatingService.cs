using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RatingService.Models;
using RatingService.Protos;
namespace RatingService.Services;

public class RatingService : Rating.RatingBase
{
    private readonly RatingDbContext _context;
    private readonly ILogger<RatingService> _logger;

    public RatingService(RatingDbContext context, ILogger<RatingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<SetUserScoreResponse> SetUserScore(SetUserScoreRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Setting user score for userId: {UserId} with score: {Score}", request.UserId, request.Score);

        try
        {
            var currentSeasonId = await GetCurrentSeasonId();
            if(currentSeasonId < 0)
            {
                _logger.LogError("No active season found.");
                return new SetUserScoreResponse { Success = false, ErrorMessage = "SEASON_NOT_FOUND"};
            }

            var userScore = await _context.UserScores.FirstOrDefaultAsync(us => us.UserId == request.UserId && us.SeasonId == currentSeasonId);
            request.Score = request.Score <= 0 ? 0 : request.Score;
            if (userScore == null)
            {
                userScore = new UserScore
                {
                    UserId = request.UserId,
                    Score = request.Score,
                    SeasonId = currentSeasonId
                };
                _context.UserScores.Add(userScore);
            }
            else
            {
                userScore.Score += request.Score;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Score updated successfully for userId: {request.UserId}");
            return new SetUserScoreResponse { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error setting user score for userId: {request.UserId}", ex);
            return new SetUserScoreResponse { Success = false, ErrorMessage = ex.Message};
        }
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<GetUserScoreResponse> GetUserScore(GetUserScoreRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Retrieving user score for userId: {UserId} in seasonId: {SeasonId}", request.UserId, request.SeasonId);

        try
        {
            int seasonId = request.SeasonId > 0 ? request.SeasonId : await GetCurrentSeasonId();
            if(seasonId < 0)
            {
                _logger.LogError("No active season found.");
                return new GetUserScoreResponse()
                {
                    ErrorMessage = "SEASON_NOT_FOUND",
                    Score = -1
                };
            }
            var userScore = await _context.UserScores.AsNoTracking().FirstOrDefaultAsync(us => us.UserId == request.UserId && us.SeasonId == seasonId);
            if(userScore is null)
            {
                _logger.LogError("User score not found.");
                return new GetUserScoreResponse()
                {
                    ErrorMessage = "SCORE_NOT_FOUND",
                    Score = -1
                };
            }

            _logger.LogInformation($"Score retrieved successfully for userId: {request.UserId}");
            return new GetUserScoreResponse { Score = userScore.Score};
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user score for userId: {UserId}", request.UserId);
            return new GetUserScoreResponse { ErrorMessage = ex.Message};
        }
    }
    
    private async Task<int> GetCurrentSeasonId()
    {
        var season = await _context.Seasons.AsNoTracking().FirstOrDefaultAsync(s => s.DateStart <= DateTime.Now && s.DateEnd >= DateTime.Now);
        if (season is null)
            return -1;
        else
            return season.SeasonId;
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<GetRatingListResponse> GetRatingList(Empty request, ServerCallContext context)
    {
        _logger.LogInformation("Getting rating list for the current season.");
        try
        {
            var currentSeasonId = await GetCurrentSeasonId();

            var scores = await _context.UserScores
                                       .Where(us => us.SeasonId == currentSeasonId)
                                       .OrderByDescending(us => us.Score)  
                                       .Select(us => new UserScoreEntry { UserId = us.UserId, Score = us.Score })
                                       .ToListAsync();

            _logger.LogInformation("Rating list retrieved successfully.");
            return new GetRatingListResponse { Scores = { scores } };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rating list.");
            return new GetRatingListResponse { ErrorMessage = ex.Message};
        }
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<GetSeasonsListResponse> GetSeasonsList(Empty request, ServerCallContext context)
    {
        _logger.LogInformation("Retrieving list of all seasons.");

        try
        {
            var seasons = await _context.Seasons.Select(s => new SeasonEntry { SeasonId = s.SeasonId, DateStart = s.DateStart.ToString(), DateEnd = s.DateEnd.ToString() })
                                                .ToListAsync();

            _logger.LogInformation("Seasons list retrieved successfully.");
            return new GetSeasonsListResponse { Seasons = { seasons } };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving seasons list.");
            return new GetSeasonsListResponse { ErrorMessage = ex.Message};
        }
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<GetRatingListResponse> GetRatingListBySeason(GetRatingListBySeasonRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting rating list for seasonId: {SeasonId}", request.SeasonId);

        try
        {
            var scores = await _context.UserScores
                                       .Where(us => us.SeasonId == request.SeasonId)
                                       .OrderByDescending(us => us.Score)
                                       .Select(us => new UserScoreEntry { UserId = us.UserId, Score = us.Score })
                                       .ToListAsync();

            _logger.LogInformation("Rating list for seasonId: {SeasonId} retrieved successfully.", request.SeasonId);
            return new GetRatingListResponse { Scores = { scores } };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rating list for seasonId: {SeasonId}", request.SeasonId);
            return new GetRatingListResponse { ErrorMessage = ex.Message };
        }
    }

}
