namespace RoomsService;

public static class ErrorMessages
{
    public static string InternalServerError { get; } = "INTERNAL_SERVER_ERROR";
    public static string GameIdNotValid { get; } = "GAME_ID_INVALID";
    public static string SubjectClaimNotFound { get; } = "SUBJECT_CLAIM_NOT_FOUND";
    public static string RoomIdNotExist { get; } = "ROOM_ID_NOT_EXIST";
    public static string NotAllowed { get; } = "NOT_ALLOWED";
}
