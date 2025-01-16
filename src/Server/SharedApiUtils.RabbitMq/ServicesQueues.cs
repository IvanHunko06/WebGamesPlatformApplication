namespace SharedApiUtils.RabbitMq;

public static class ServicesQueues
{
    public static string GamesServiceQueue { get; } = "GamesService";
    public static string RoomsServiceQueue { get; } = "RoomsService";
    public static string RatingServiceQueue { get; } = "RatingService";
    public static string MatchHistoryServiceQueue { get; } = "MatchHistoryService";
    public static string GameSessionServiceQueue { get; } = "GameSessionService";
    public static string GameSessionWsNotifyer { get; } = "GameSessionWsNotifyer";
}
