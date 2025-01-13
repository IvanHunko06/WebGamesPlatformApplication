namespace TicTacToeGameProcessing;

public static class GameError
{
    public static string UnknownAction { get; } = "UNKNOWN_ACTION";
    public static string NotYourTurn { get; } = "NOT_YOUR_TURN";
    public static string CellIsOccupied { get; } = "CELL_IS_OCCUPIED";
    public static string InvalidPayload { get; } = "INVALID_PAYLOAD";
    public static string IncorrectCellIdForPut { get; } = "INCORRECT_CELL_ID_FOR_PUT";
    public static string InvalidSessionState { get; } = "INVALID_SESSION_STATE";
}
