namespace MatchHistoryService;

public static class ErrorMessages
{
    public static string InternalServerError { get; } = "INTERNAL_SERVER_ERROR";
    public static string MatchIdNotExist { get; } = "MATCH_ID_NOT_EXIST";
    public static string PlayerIdNotExist { get; } = "PLAYER_ID_NOT_EXIST";
    public static string InvalidDataFormat { get; } = "INVALID_DATA_FORMAT";
    public static string InvalidMatchInfromation { get; } = "INVALID_MATCH_INFORMATION";
    public static string InsufficientData { get; } = "INSUFFICIENT_DATA";
    public static string InvalidMatchInformation { get; } = "INVALID_MATCH_INFORMATION";
    public static string NotFound { get; } = "NOT_FOUND";
}