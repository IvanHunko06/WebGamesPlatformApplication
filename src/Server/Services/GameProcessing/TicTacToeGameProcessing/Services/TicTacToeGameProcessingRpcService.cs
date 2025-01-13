using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;
using TicTacToeGameProcessing.Interfaces;
namespace TicTacToeGameProcessing.Services;

[Authorize(Policy = "OnlyPrivateClient")]
public class TicTacToeGameProcessingRpcService : GameProcessing.GameProcessingBase
{
    private readonly ITicTacToeGameProcessingService ticTacToeGameProcessingService;

    public TicTacToeGameProcessingRpcService(ITicTacToeGameProcessingService ticTacToeGameProcessingService)
    {
        this.ticTacToeGameProcessingService = ticTacToeGameProcessingService;
    }
    public override Task<InitEmptySessionStateReply> InitEmptySessionState(InitEmptySessionStateRequest request, ServerCallContext context)
    {
        string sessionState = ticTacToeGameProcessingService.GetEmptySessionState(new List<string>(request.Players));
        var reply = new InitEmptySessionStateReply()
        {
            SessionStateObject = sessionState,
        };
        return Task.FromResult(reply);
    }
    public override Task<ProccessActionReply> ProccessAction(ProccessActionRequest request, ServerCallContext context)
    {

        ProccessActionReply reply = new ProccessActionReply();
        var updateReply = ticTacToeGameProcessingService.UpdateSessionState(request.SessionStateObject, request.PlayerId, request.Action, request.Payload);
        if (!string.IsNullOrEmpty(updateReply.gameErrorMessage))
        {
            reply.GameErrorMessage = updateReply.gameErrorMessage;
            reply.NewSessionState = request.SessionStateObject;
            return Task.FromResult(reply);
        }
        reply.IsSuccess = true;
        reply.NewSessionState = updateReply.newSessionState;
        return Task.FromResult(reply);
    }
}
