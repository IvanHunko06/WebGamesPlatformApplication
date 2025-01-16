using System.Text.Json;
using SharedApiUtils.Abstractons;
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
        if (players.Count != 2)
            return ErrorMessages.IncorrectGamePlayersCount;
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
    public (string? notifySession, Dictionary<string, string>? notifyPlayers) GetSessionDeltaMessages(string oldSessionState, string newSessionState)
    {
        try
        {
            TicTacToeSessionState? oldState = JsonSerializer.Deserialize<TicTacToeSessionState>(oldSessionState);
            TicTacToeSessionState? newState = JsonSerializer.Deserialize<TicTacToeSessionState>(newSessionState);
            if (oldState is null || newState is null)
                return (null, null);
            NotifyClientAction? updateCurrentPlayer = null;
            if (oldState.CurrentPlayer != newState.CurrentPlayer)
            {
                updateCurrentPlayer = new NotifyClientAction()
                {
                    Action = "UpdateCurrentPlayer",
                    Payload = newState.CurrentPlayer,
                };
            }
            NotifyClientAction? updateGameBoard = null;
            if (!oldState.GameBoard.SequenceEqual(newState.GameBoard))
            {
                updateGameBoard = new NotifyClientAction()
                {
                    Action = "UpdateGameboard",
                    Payload = JsonSerializer.Serialize(newState.GameBoard)
                };
            }
            List<NotifyClientAction> totalChanges = new List<NotifyClientAction>();
            if (updateCurrentPlayer is not null)
                totalChanges.Add(updateCurrentPlayer);
            if (updateGameBoard is not null)
                totalChanges.Add(updateGameBoard);

            if (totalChanges.Count == 0)
                return (null, null);
            else
                return (JsonSerializer.Serialize(totalChanges), null);
        }
        catch (Exception)
        {
            return (null, null);
        }
    }
    public (bool IsOver, Dictionary<string, int>? PlayerScores) CheckGameWin(string jsonSessionState)
    {
        try
        {
            TicTacToeSessionState? sessionState = JsonSerializer.Deserialize<TicTacToeSessionState>(jsonSessionState);
            if (sessionState is null)
                return (false, null);
            int[][] winningCombinations = new int[][]
            {
            new int[] {0, 1, 2}, new int[] {3, 4, 5}, new int[] {6, 7, 8},
            new int[] {0, 3, 6}, new int[] {1, 4, 7}, new int[] {2, 5, 8},
            new int[] {0, 4, 8}, new int[] {2, 4, 6}
            };

            string? winPlayer = null;
            foreach (var combo in winningCombinations)
            {
                if (sessionState.GameBoard[combo[0]] != ' ' &&
                    sessionState.GameBoard[combo[0]] == sessionState.GameBoard[combo[1]] &&
                    sessionState.GameBoard[combo[1]] == sessionState.GameBoard[combo[2]])
                {
                    winPlayer = sessionState.GameBoard[combo[0]] == 'X' ? sessionState.XPlayer : sessionState.OPlayer;
                }
            }
            if (!string.IsNullOrEmpty(winPlayer))
            {
                Dictionary<string, int> scores = new Dictionary<string, int>();
                scores[sessionState.XPlayer] = winPlayer == sessionState.XPlayer ? 10 : -10;
                scores[sessionState.OPlayer] = winPlayer == sessionState.OPlayer ? 10 : -10;
                return (true, scores);
            }
            else if (sessionState.GameBoard.Contains(' '))
            {
                return (false, null);
            }
            else
            {
                Dictionary<string, int> scores = new Dictionary<string, int>();
                scores[sessionState.XPlayer] = 0;
                scores[sessionState.OPlayer] = 0;
                return (true, scores);
            }
        }
        catch (Exception)
        {
            return (false, null);
        }
    }
}
