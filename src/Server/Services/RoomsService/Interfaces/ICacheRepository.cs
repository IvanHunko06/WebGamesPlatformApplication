using SharedApiUtils.Abstractons.Core;

namespace RoomsService.Interfaces
{
    public interface ICacheRepository
    {
        Task CacheGameInfo(string gameId, GameInfoDto gameInfo);
        Task DeleteCachedGameInfo(string gameId);
        Task<GameInfoDto?> GetCachedGameInfo(string gameId);
    }
}