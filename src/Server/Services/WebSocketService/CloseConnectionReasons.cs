namespace WebSocketService;

public static class CloseConnectionReasons
{
    public static string InvalidJwtToken { get; set; } = "INVALID_AUTH_TOKEN";
    public static string ConnectionAlreadyEstablished { get; set; } = "CONNECTION_ALREADY_ESTABLISHED";
}
