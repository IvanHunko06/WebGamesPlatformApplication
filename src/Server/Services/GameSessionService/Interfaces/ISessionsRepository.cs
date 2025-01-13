using GameSessionService.Models;

namespace GameSessionService.Interfaces;

public interface ISessionsRepository
{
    Task AddOrUpdateSession(GameSessionModel session);
    Task DeleteSessionById(string sessionId);
    Task<GameSessionModel?> GetSessionById(string sessionId);
    Task<List<string>> GetSessionIdsList();
    Task<List<Models.GameSessionModel>> GetSessionsList();
}