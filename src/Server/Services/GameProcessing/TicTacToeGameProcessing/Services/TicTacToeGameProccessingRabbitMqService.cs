using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Core.Messages.GameProccessingService;
using SharedApiUtils.RabbitMq.Listeners;
using TicTacToeGameProcessing.Interfaces;

namespace TicTacToeGameProcessing.Services;

public sealed class TicTacToeGameProccessingRabbitMqService : BaseRabbitMqGameProccessingListener
{
    private readonly ITicTacToeGameProcessingService ticTacToeGameProcessingService;

    public TicTacToeGameProccessingRabbitMqService(
        ITicTacToeGameProcessingService ticTacToeGameProcessingService,
        RabbitMqConnection _connection, 
        ILogger<BaseRabbitMqGameProccessingListener> _logger1,
        ILogger<BaseRabbitMqMessageListener> _logger2) :base(_logger1, _connection, _logger2)
    {
        this.ticTacToeGameProcessingService = ticTacToeGameProcessingService;
    }
    protected override Task<GetEmptySessionStateReply> GetEmptySessionState(GetEmptySessionStateRequest request)
    {
        string sessionState = ticTacToeGameProcessingService.GetEmptySessionState(request.Players.ToList());
        var reply = new GetEmptySessionStateReply();
        reply.SessionState = sessionState;
        return Task.FromResult(reply);
    }

    protected override Task<GetGameStateForPlayerReply> GetGameStateForPlayer(GetGameStateForPlayerRequest request)
    {
        var reply = new GetGameStateForPlayerReply();
        reply.GameState = ticTacToeGameProcessingService.GetGameStateForPlayer(request.GameState, request.UserId);
        return Task.FromResult(reply);
    }

    protected override Task<ProccessActionReply> ProccessAction(ProccessActionRequest request)
    {
        var updateResult = ticTacToeGameProcessingService.UpdateSessionState(request.SessionState, request.UserId, request.Action, request.Payload);
        var reply = new ProccessActionReply();
        if (!string.IsNullOrEmpty(updateResult.gameErrorMessage))
        {
            reply.GameErrorMessage = updateResult.gameErrorMessage;
            reply.NewSessionState = request.SessionState;
            return Task.FromResult(reply);
        }
        reply.GameErrorMessage = null;
        reply.NewSessionState = updateResult.newSessionState ?? "";
        reply.IsSuccess = true;
        return Task.FromResult(reply);
    }
}
