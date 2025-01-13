namespace SharedApiUtils.Abstractons;

public static class ErrorMessages
{
    public static string InternalServerError { get; } = "INTERNAL_SERVER_ERROR";
    public static string TimeoutExceeded { get; } = "TIMEOUT_EXCEEDED";
    public static string ConnectionBlocked { get; } = "CONNECTION_BLOCKED";

    public static string GameIdNotValid { get; } = "GAME_ID_INVALID";
    public static string SubjectClaimNotFound { get; } = "SUBJECT_CLAIM_NOT_FOUND";
    public static string RoomIdNotExist { get; } = "ROOM_ID_NOT_EXIST";
    public static string NotAllowed { get; } = "NOT_ALLOWED";


    public static string DublicateGameId { get; } = "DUBLICATED_GAME_ID";
    public static string PropertyTooLong { get; } = "PROPERTY_TOO_LONG";

    public static string InvalidDateFormat { get; } = "INVALID_DATE_FORMAT";
    public static string NotFound { get; } = "NOT_FOUND";
    public static string SeasonNotFound { get; } = "SEASON_NOT_FOUND";
    public static string NoCurrentSeason { get; } = "NO_CURRENT_SEASON";
    public static string NoUserScore { get; } = "NO_USER_SCORE";

    public static string RoomIsFull { get; } = "ROOM_IS_FULL";
    public static string RoomIsNotFull { get; } = "ROOM_IS_NOT_FULL";
    public static string AlreadyInRoom { get; } = "ALREADY_IN_ROOM";
    public static string NotInRoom { get; } = "NOT_IN_ROOM";
    public static string UserAsignedToRoom { get; } = "USER_ALREADY_ASSIGNED_TO_ROOM";

    
    public static string SessionIdNotExist { get; } = "SESSION_ID_NOT_EXIST";
    public static string SessionForRoomExist { get; } = "SESSION_FOR_ROOM_ALREADY_EXIST";
    public static string GameInProgress { get; } = "GAME_IN_PROGRESS";
    public static string UserNotInSession { get; } = "USER_NOT_IN_SESSION";
}
