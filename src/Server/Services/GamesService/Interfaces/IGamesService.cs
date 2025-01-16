using GamesService.Models;

namespace GamesService.Interfaces
{
    public interface IGamesService
    {
        Task<string?> AddGame(GameInfoModel gameDto);
        Task<string?> DeleteGame(string gameId);
        Task<List<GameInfoModel>?> GetGamesList();
        Task<string?> UpdateGame(GameInfoModel gameInfo);
    }
}