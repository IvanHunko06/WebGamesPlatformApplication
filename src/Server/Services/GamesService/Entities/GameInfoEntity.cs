namespace GamesService.Models;

public class GameInfoEntity
{
    public int Id { get; set; }
    public string GameId {  get; set; } = string.Empty;
    public string LocalizationKey { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool SupportSinglePlayer {  get; set; }
    public bool StaticPlayersCount { get; set; }
    public int MinPlayersCount {  get; set; }
    public int MaxPlayersCount {  get; set; }
    public string GameLogicServerUrl { get; set; } = string.Empty;
}
