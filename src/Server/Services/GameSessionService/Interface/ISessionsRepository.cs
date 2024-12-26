using GameSessionService.Models;

namespace GameSessionService.Interface;

public interface ISessionsRepository
{
    Task AddOrUpdateSession(GameSession session);
    Task DeleteSessionById(string sessionId);
    Task<GameSession?> GetSessionById(string sessionId);
    Task<List<string>> GetSessionIdsList();
    Task<List<Models.GameSession>> GetSessionsList();
}