using MatchHistoryService.Entities;

namespace MatchHistoryService.Interfaces
{
    public interface IMatchInfoRepository
    {
        Task AddMatchInfo(MatchInformationEntity matchInformation);
        Task DeleteMatchInfo(Guid recordId);
        Task<List<MatchInformationEntity>> GetAll();
        Task<List<MatchInformationEntity>> GetAllForUser(string userId);
    }
}