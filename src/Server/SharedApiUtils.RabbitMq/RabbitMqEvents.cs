namespace SharedApiUtils.RabbitMq;

public static class RabbitMqEvents
{
    public const string OnRoomCreated  = "OnRoomCreated";
    public const string OnRoomDeleted  = "OnRoomDeleted";
    public const string OnRoomJoin     = "OnRoomJoin";
    public const string OnRoomLeave    = "OnRoomLeave";


    public const string AddUserToRoom = "AddUserToRoom";
    public const string RemoveUserFromRoom = "RemoveUserFromRoom";
    public const string DeleteRoom = "DeleteRoom";
    public const string GetRoom = "GetRoom";


    public const string EndGameSession = "EndGameSession";
    public const string GetGameSession = "GetGameSession";
    public const string SendGameEvent = "SendGameEvent";
    public const string StartGameSession = "StartGameSession";
    public const string SyncGameState = "SyncGameState";


    public const string GetEmptySessionState = "GetEmptySessionState";
    public const string ProccessAction = "ProccessAction";
    public const string GetGameStateForPlayer = "GetGameStateForPlayer";
    public const string GetSessionDeltaMessages = "GetSessionDeltaMessages";
    public const string CheckGameOver = "CheckGameOver";


    public const string NotifyReciveAction_AllUsers = "NotifyReciveAction_AllUsers";
    public const string NotifyReciveAction_User = "NotifyReciveAction_User";
    public const string NotifySessionEnded_User = "NotifySessionEnded_User";
    public const string NotifySessionEnded_AllUsers = "NotifySessionEnded_AllUsers";


    public const string AddLastSeasonUserScore = "AddLastSeasonUserScore";


    public const string AddMatchInfo = "AddMatchInfo";
}
