using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using SharedApiUtils.ServicesAccessing.Protos;
using System.Text.Json;
using TicTacToeGameProcessing.Models;
namespace TicTacToeGameProcessing.Services;

[Authorize(Policy = "OnlyPrivateClient")]
public class TicTacToeGameProcessingService : GameProcessing.GameProcessingBase
{
    private readonly Random random;
    private readonly ILogger<TicTacToeGameProcessingService> logger;

    public TicTacToeGameProcessingService(Random random, ILogger<TicTacToeGameProcessingService> logger)
    {
        this.random = random;
        this.logger = logger;
    }
    public override Task<InitEmptySessionStateReply> InitEmptySessionState(InitEmptySessionStateRequest request, ServerCallContext context)
    {
        InitEmptySessionStateReply reply = new InitEmptySessionStateReply();
        TicTacToeSessionState sessionState = new TicTacToeSessionState()
        {
            GameBoard = new List<char> { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '}
        };
        int randomCurrentSymbol = random.Next(0, 2);
        sessionState.CurrentPlayer = request.Players[randomCurrentSymbol];

        int randomXPlayer = random.Next(0, 2);
        sessionState.XPlayer = request.Players[randomXPlayer];
        sessionState.OPlayer = request.Players[1-randomXPlayer];

        reply.JsonSessionStateObject = JsonSerializer.Serialize(sessionState);

        return Task.FromResult(reply);
    }
    public override Task<ProccessActionReply> ProccessAction(ProccessActionRequest request, ServerCallContext context)
    {
        logger.LogInformation($"PlayerId: {request.PlayerId}  Action: {request.Action}  Payload: {request.Payload}  CurrentState: {request.JsonSessionStateObject}");
        ProccessActionReply reply = new ProccessActionReply();
        TicTacToeSessionState currentSessionState = JsonSerializer.Deserialize<TicTacToeSessionState>(request.JsonSessionStateObject);
        if(request.Action == "PUT")
        {
            PutRequestPayload? payload = null;
            try
            {
                if (string.IsNullOrEmpty(request.Payload))
                {
                    reply.IsSuccess = false;
                    reply.NewSessionState = request.JsonSessionStateObject;
                    reply.GameErrorMessage = GameError.InvalidPayload;
                    return Task.FromResult(reply);
                }
                payload = JsonSerializer.Deserialize<PutRequestPayload>(request.Payload);
                
            }
            catch (JsonException)
            {
                reply.IsSuccess = false;
                reply.NewSessionState = request.JsonSessionStateObject;
                reply.GameErrorMessage = GameError.InvalidPayload;
                return Task.FromResult(reply);
            }
            if (currentSessionState.CurrentPlayer != request.PlayerId)
            {
                reply.IsSuccess = false;
                reply.NewSessionState = request.JsonSessionStateObject;
                reply.GameErrorMessage = GameError.NotYourTurn;
                return Task.FromResult(reply);
            }
            if(payload.CellId < 0 || payload.CellId > 8)
            {
                reply.IsSuccess = false;
                reply.NewSessionState = request.JsonSessionStateObject;
                reply.GameErrorMessage = GameError.IncorrectCellIdForPut;
                return Task.FromResult(reply);
            }
            if (currentSessionState.GameBoard[payload.CellId] != ' ')
            {
                reply.IsSuccess = false;
                reply.NewSessionState = request.JsonSessionStateObject;
                reply.GameErrorMessage = GameError.CellIsOccupied;
                return Task.FromResult(reply);
            }
            var newSessionState = new TicTacToeSessionState(currentSessionState);
            newSessionState.CurrentPlayer = currentSessionState.CurrentPlayer == currentSessionState.XPlayer ? 
                currentSessionState.OPlayer : currentSessionState.XPlayer;
            newSessionState.GameBoard[payload.CellId] = currentSessionState.CurrentPlayer == currentSessionState.XPlayer ? 'X' : 'O';
            reply.IsSuccess = true;
            reply.NewSessionState = JsonSerializer.Serialize(newSessionState);
            NotifyClientAction updateGameBoard = new NotifyClientAction()
            {
                Action = "UpdateGameboard",
                Payload = JsonSerializer.Serialize(newSessionState.GameBoard)
            };
            NotifyClientAction updateCurrentPlayer = new NotifyClientAction()
            {
                Action = "UpdateCurrentPlayer",
                Payload = newSessionState.CurrentPlayer
            };
            List<NotifyClientAction> notifyActions = new List<NotifyClientAction>()
            {
                updateGameBoard,
                updateCurrentPlayer
            };
            reply.NotifyRoom = JsonSerializer.Serialize(notifyActions);
            return Task.FromResult(reply);
        }
        else
        {
            reply.IsSuccess = false;
            reply.NewSessionState = request.JsonSessionStateObject;
            reply.GameErrorMessage = GameError.UnknownAction;
            return Task.FromResult(reply);
        }
    }
}
