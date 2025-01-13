using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using RatingService.Interfaces;
using SharedApiUtils.Abstractons;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;
namespace RatingService.Services;

public class RatingRpcService : Rating.RatingBase
{
    private readonly IRatingService ratingService;

    public RatingRpcService(RatingService ratingService)
    {

        this.ratingService = ratingService;
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    public async override Task<SetLastSeasonUserScoreReply> SetLastSeasonUserScore(SetLastSeasonUserScoreRequest request, ServerCallContext context)
    {
        var response = new SetLastSeasonUserScoreReply();
        var season = await ratingService.GetCurrentSeason();
        if (season is null)
        {
            response.ErrorMessage = ErrorMessages.NoCurrentSeason;
            return response;
        }

        string? errorMessage = await ratingService.AddOrUpdateUserScore(season.SeasonId, request.UserId, request.NewScore);
        if (string.IsNullOrEmpty(errorMessage))
            response.IsSuccess = true;
        else
            response.ErrorMessage = errorMessage;
        return response;
    }


    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<GetUserScoreReply> GetUserScore(GetUserScoreRequest request, ServerCallContext context)
    {
        var reply = new GetUserScoreReply();
        var score = await ratingService.GetUserScore(request.UserId, request.SeasonId);
        if (score is null)
        {
            reply.ErrorMessage = ErrorMessages.NoUserScore;
        }
        else
        {
            reply.IsSuccess = true;
            reply.Score = score.Value;
        }
        return reply;
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<GetRatingListReply> GetRatingList(GetRatingListRequest request, ServerCallContext context)
    {
        var reply = new GetRatingListReply();
        var ratingList = await ratingService.GetRatingList(request.SeasonId);
        reply.UserScores.AddRange(ratingList.Select(x =>
        {
            return new UserScoreEntry()
            {
                Score = x.Score,
                UserId = x.UserId,
            };
        }).ToList());
        return reply;
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<GetSeasonsListReply> GetSeasonsList(Empty request, ServerCallContext context)
    {
        var reply = new GetSeasonsListReply();
        var seasons = await ratingService.GetAllSeasons();
        reply.Seasons.AddRange(seasons.Select(x => new SeasonEntry()
        {
            DateEnd = x.EndDate.ToShortDateString(),
            DateStart = x.BeginDate.ToShortDateString(),
            SeasonId = x.SeasonId,
        }).ToList());
        return reply;
    }
}
