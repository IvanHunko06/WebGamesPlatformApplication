using MatchHistoryService.Models;

namespace MatchHistoryService.Interfaces;

public interface IMatchHistoryService
{
    Task<string?> AddMatchInfo(MatchInfoModel matchInfo);
    Task<string?> DeleteMatchInfo(string recordId);
    Task<List<MatchInfoModel>?> GetAllMatchesForUser(string userId);
    Task<List<MatchInfoModel>?> GetAllRecords();
}