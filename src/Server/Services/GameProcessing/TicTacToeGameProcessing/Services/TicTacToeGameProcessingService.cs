using Microsoft.Extensions.Logging;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;
using System;
using System.Text.Json;
using TicTacToeGameProcessing.Interfaces;
using TicTacToeGameProcessing.Models;

namespace TicTacToeGameProcessing.Services;

public class TicTacToeGameProcessingService : ITicTacToeGameProcessingService
{
    private readonly Random random;
    private readonly ILogger<TicTacToeGameProcessingService> logger;

    public TicTacToeGameProcessingService(Random random, ILogger<TicTacToeGameProcessingService> logger)
    {
        this.random = random;
        this.logger = logger;
    }
    public string GetEmptySessionState(List<string> players)
    {
        TicTacToeSessionState sessionState = new TicTacToeSessionState()
        {
            GameBoard = new List<char> { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' }
        };
        int randomCurrentSymbol = random.Next(0, 2);
        sessionState.CurrentPlayer = players[randomCurrentSymbol];

        int randomXPlayer = random.Next(0, 2);
        sessionState.XPlayer = players[randomXPlayer];
        sessionState.OPlayer = players[1 - randomXPlayer];
        return JsonSerializer.Serialize(sessionState);

    }
    public (string? gameErrorMessage, string? newSessionState) UpdateSessionState(string sessionState, string userId, string action, string payload)
    {
        logger.LogInformation($"UserId: {userId}  Action: {action}  Payload: {payload}  CurrentState: {sessionState}");
        TicTacToeSessionState? currentSessionState = JsonSerializer.Deserialize<TicTacToeSessionState>(sessionState);
        if (currentSessionState is null)
            return (GameError.InvalidSessionState, null);
        if (action == "PUT")
        {
            PutRequestPayload? putRequestPayload = null;
            try
            {
                if (string.IsNullOrEmpty(payload))
                    return (GameError.InvalidPayload, null);
                putRequestPayload = JsonSerializer.Deserialize<PutRequestPayload>(payload);
                if (putRequestPayload is null)
                    return (GameError.InvalidPayload, null);

            }
            catch (JsonException)
            {
                return (GameError.InvalidPayload, null);
            }
            if (currentSessionState.CurrentPlayer != userId)
                return (GameError.NotYourTurn, null);

            if (putRequestPayload.CellId < 0 || putRequestPayload.CellId > 8)
                return (GameError.IncorrectCellIdForPut, null);
            if (currentSessionState.GameBoard[putRequestPayload.CellId] != ' ')
                return (GameError.CellIsOccupied, null);

            var newSessionState = new TicTacToeSessionState(currentSessionState);
            newSessionState.CurrentPlayer = currentSessionState.CurrentPlayer == currentSessionState.XPlayer ?
                currentSessionState.OPlayer : currentSessionState.XPlayer;
            newSessionState.GameBoard[putRequestPayload.CellId] = currentSessionState.CurrentPlayer == currentSessionState.XPlayer ? 'X' : 'O';
            return (null, JsonSerializer.Serialize(newSessionState));
        }
        else
        {
            return (GameError.UnknownAction, null);
        }
    }
    public string GetGameStateForPlayer(string sessionState, string userId)
    {
        return sessionState;
    }
}
