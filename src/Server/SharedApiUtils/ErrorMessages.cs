namespace SharedApiUtils;

public static class ErrorMessages
{
    public static string InternalServerError { get; } = "INTERNAL_SERVER_ERROR";


    public static string GameIdNotValid { get; } = "GAME_ID_INVALID";
    public static string SubjectClaimNotFound { get; } = "SUBJECT_CLAIM_NOT_FOUND";
    public static string RoomIdNotExist { get; } = "ROOM_ID_NOT_EXIST";
    public static string NotAllowed { get; } = "NOT_ALLOWED";


    public static string DublicateGameId { get; } = "DUBLICATED_GAME_ID";
    public static string PropertyTooLong { get; } = "PROPERTY_TOO_LONG";

    public static string InvalidDateFormat { get; } = "INVALID_DATE_FORMAT";
    public static string NotFound { get; } = "NOT_FOUND";

    public static string RoomIsFull { get; } = "ROOM_IS_FULL";
    public static string AlreadyInRoom { get; } = "ALREADY_IN_ROOM";
    public static string NotInRoom { get; } = "NOT_IN_ROOM";
    public static string UserAsignedToRoom { get; } = "USER_ALREADY_ASSIGNED_TO_ROOM";
}
