using Microsoft.Extensions.Logging;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.RabbitMq.Core.Messages.RatingService;

namespace SharedApiUtils.RabbitMq.Clients;

public class RabbitMqRatingServiceClient : RabbitMqBaseClient, IRatingServiceClient
{
    private readonly ILogger<RabbitMqRatingServiceClient> logger;

    public RabbitMqRatingServiceClient(
        ILogger<RabbitMqRatingServiceClient> logger,
        RabbitMqConnection _connection,
        ILogger<RabbitMqBaseClient> _logger) : base(_connection, _logger)
    {
        this.logger = logger;
    }
    public async Task<string?> AddLastSeasonUserScore(string userId, int addScore)
    {
        try
        {
            var request = new AddLastSeasonUserScoreRequest { UserId = userId, AddScore = addScore };
            var reply = await SendRequest<AddLastSeasonUserScoreRequest, AddLastSeasonUserScoreReply>(request, RabbitMqEvents.AddLastSeasonUserScore, ServicesQueues.RatingServiceQueue);
            return reply.IsSuccess ? null : reply.ErrorMessage;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to add last season user score.");
            return ErrorMessages.InternalServerError;
        }
    }
}
