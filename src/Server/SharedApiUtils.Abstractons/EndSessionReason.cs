namespace SharedApiUtils.Abstractons;

public static class EndSessionReason
{
    public static string PlayerDisconnected { get; } = "PLAYER_DISCONNECTED";
    public static string DebugDelete { get; } = "DEBUG_DELETE";
    public static string RoomDeleted { get; } = "ROOM_DELETED";
    public static string NormalFinish { get; } = "NORMAL_FINISH";
}
