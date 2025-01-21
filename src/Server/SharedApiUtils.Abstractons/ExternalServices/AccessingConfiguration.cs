namespace SharedApiUtils.Abstractons.ExternalServices;

public class AccessingConfiguration
{
    public string? GamesServiceUrl { get; set; } = null;
    public string? RoomsServiceUrl { get; set; } = null;
    public string? RatingServiceUrl { get; set; } = null;
    public string? MatchHistoryServiceUrl { get; set; } = null;
    public string? WebSocketServiceUrl { get; set; } = null;
    public string? RoomsEventsHandlerUrl {  get; set; } = null;
    public string? GameSessionServiceUrl { get; set; } = null;
    public string? ProfileServiceUrl {  get; set; } = null;
    public bool IgnoreSslVerification { get; set; }
}
