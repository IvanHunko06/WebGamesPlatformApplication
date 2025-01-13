using RatingService.Models;

namespace RatingService.Interfaces
{
    public interface IRatingService
    {
        Task<string?> AddOrUpdateUserScore(int seasonId, string userId, int score);
        Task<string?> AddSeason(DateOnly beginDate, DateOnly endDate);
        Task<List<SeasonModel>> GetAllSeasons();
        Task<SeasonModel?> GetCurrentSeason();
        Task<List<UserScoreModel>> GetRatingList(int seasonId, int SelectedRecords = 10);
        Task<int?> GetUserScore(string userId, int seasonId);
    }
}