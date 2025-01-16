using MatchHistoryService.Interfaces;
using MatchHistoryService.Models;
using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Core.Messages.MatchHistoryService;
using SharedApiUtils.RabbitMq.Listeners;

namespace MatchHistoryService.Services;

public class MatchHistoryRabbitMqService : BaseRabbitMqMatchHistoryServiceListener
{
    private readonly IMatchHistoryService matchHistoryService;

    public MatchHistoryRabbitMqService(
        IMatchHistoryService matchHistoryService,
        ILogger<BaseRabbitMqMatchHistoryServiceListener> _logger1,
        RabbitMqConnection _connection,
        ILogger<BaseRabbitMqMessageListener> _logger2) : base(_logger1, _connection, _logger2)
    {
        this.matchHistoryService = matchHistoryService;
    }

    protected override async Task<AddMatchInfoReply> AddMatchInfo(AddMatchInfoRequest request)
    {
        var reply = new AddMatchInfoReply();

        MatchInfoModel matchInfo = new MatchInfoModel()
        {
            FinishReason = request.FinishReason,
            GameId = request.GameId,
            RecordId = Guid.NewGuid(),
            TimeBegin = request.StartTime,
            TimeEnd = request.EndTime,
            UserScoreDelta = request.PlayerScoresDelta,
        };
        string? errorMessage = await matchHistoryService.AddMatchInfo(matchInfo);
        if (string.IsNullOrEmpty(errorMessage))
            reply.IsSuccess = true;
        else 
            reply.ErrorMessage = errorMessage;

        return reply;
    }
}
