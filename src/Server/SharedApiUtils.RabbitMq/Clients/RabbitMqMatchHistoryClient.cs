using Microsoft.Extensions.Logging;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.RabbitMq.Core.Messages.MatchHistoryService;

namespace SharedApiUtils.RabbitMq.Clients;

public class RabbitMqMatchHistoryClient : RabbitMqBaseClient, IMatchHistoryServiceClient
{
    private readonly ILogger<RabbitMqMatchHistoryClient> logger;

    public RabbitMqMatchHistoryClient(
        ILogger<RabbitMqMatchHistoryClient> logger,
        RabbitMqConnection _connection, 
        ILogger<RabbitMqBaseClient> _logger) : base(_connection, _logger)
    {
        this.logger = logger;
    }
    public async Task<string?> AddMatchInfo(string gameId, string finishReason, DateTimeOffset startTime, DateTimeOffset endTime, Dictionary<string, int> playerScoresDelta)
    {
        try
        {
            var request = new AddMatchInfoRequest { GameId = gameId, FinishReason = finishReason, StartTime = startTime, EndTime = endTime, PlayerScoresDelta = playerScoresDelta };
            var reply = await SendRequest<AddMatchInfoRequest, AddMatchInfoReply>(request, RabbitMqEvents.AddMatchInfo, ServicesQueues.MatchHistoryServiceQueue);
            return reply.IsSuccess ? null : reply.ErrorMessage;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to add match info.");
            return ErrorMessages.InternalServerError;
        }
    }
}
