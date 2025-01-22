using RatingService.Interfaces;
using SharedApiUtils.Abstractons;
using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Core.Messages.RatingService;
using SharedApiUtils.RabbitMq.Listeners;

namespace RatingService.Services;

public class RatingRabbitMqService : BaseRabbitMqRatingServiceListener
{
    private readonly ILogger<RatingRabbitMqService> logger;
    private readonly IRatingService ratingService;

    public RatingRabbitMqService(
        ILogger<RatingRabbitMqService> logger,
        ILogger<BaseRabbitMqRatingServiceListener> _logger1,
        RabbitMqConnection _connection,
        ILogger<BaseRabbitMqMessageListener> _logger2,
        IRatingService ratingService) : base(_logger1, _connection, _logger2)
    {
        this.logger = logger;
        this.ratingService = ratingService;
    }

    protected override async Task<AddLastSeasonUserScoreReply> AddLastSeasonUserScore(AddLastSeasonUserScoreRequest request)
    {
        logger.LogInformation($"Adding {request.AddScore} to {request.UserId} score");

        var reply = new AddLastSeasonUserScoreReply();
        var season = await ratingService.GetCurrentSeason();
        if(season is null)
        {
            reply.ErrorMessage = ErrorMessages.NoCurrentSeason;
            return reply;
        }
        int? currentScore = await ratingService.GetUserScore(request.UserId, season.SeasonId);
        if (currentScore is null)
            currentScore = request.AddScore;
        else
            currentScore += request.AddScore;

        if (currentScore.Value < 0)
            currentScore = 0;
        string? errorMessage = await ratingService.AddOrUpdateUserScore(season.SeasonId, request.UserId, currentScore.Value);
        if(string.IsNullOrEmpty(errorMessage))
            reply.IsSuccess = true;
        else
            reply.ErrorMessage = errorMessage;
        return reply;
    }
}
