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
}
