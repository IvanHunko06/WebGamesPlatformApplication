namespace SharedApiUtils.Abstractons.Core;

public class GameInfoDto
{
    public string GameId { get; set; } = string.Empty;
    public bool SupportSinglePlayer { get; set; }
    public bool StaticPlayersCount { get; set; }
    public int MinPlayersCount { get; set; }
    public int MaxPlayersCount { get; set; }
}
