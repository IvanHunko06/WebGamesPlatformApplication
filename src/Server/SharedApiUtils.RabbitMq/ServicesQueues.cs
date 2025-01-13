namespace SharedApiUtils.RabbitMq;

public static class ServicesQueues
{
    public static string GamesServiceQueue { get; } = "GamesService";
    public static string RoomsServiceQueue { get; } = "RoomsService";
    public static string GameSessionServiceQueue { get; } = "GameSessionService";
}
