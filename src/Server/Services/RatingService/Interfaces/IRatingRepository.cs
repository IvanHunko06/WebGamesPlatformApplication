using RatingService.Entities;

namespace RatingService.Interfaces;

public interface IRatingRepository
{
    Task AddSeason(SeasonEntity season);
    Task AddUserScore(UserScoreEntity userScore);
    Task<List<SeasonEntity>> GetAllSeasons();
    Task<SeasonEntity?> GetSeasonById(int seasonId);
    Task<SeasonEntity?> GetSeasonForDate(DateOnly date);
    Task<UserScoreEntity?> GetUserScore(string userId, int seasonId);
    Task<List<UserScoreEntity>> GetUserScoresList(int seasonId, int selectedRecords);
    Task UpdateUserScore(string userId, int seasonId, int newScore);
}