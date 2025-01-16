namespace GamesService.Models;

public class GameInfoModel
{
    public string GameId              { get; set; }
    public string ImageUrl            {  get; set; }
    public string LocalizationKey     {  get; set; }
    public int    MaxPlayersCount     {  get; set; }
    public int    MinPlayersCount     {  get; set; }
    public bool   StaticPlayersCount  { get; set; }
    public bool   SupportSinglePlayer { get; set; }
}
