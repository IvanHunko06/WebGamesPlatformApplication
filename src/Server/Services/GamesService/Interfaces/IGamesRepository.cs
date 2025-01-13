using GamesService.Models;

namespace GamesService.Interfaces;

public interface IGamesRepository
{
    Task AddGame (GameInfoEntity gameInfo);
    Task DeleteGame(string gameId);
    Task UpdateGame(GameInfoEntity gameInfo);
    Task<GameInfoEntity?> GetGameById (string gameId);
    Task<List<GameInfoEntity>> GetAllGames();
}
