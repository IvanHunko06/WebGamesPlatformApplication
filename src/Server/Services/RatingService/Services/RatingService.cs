using RatingService.Entities;
using RatingService.Interfaces;
using RatingService.Models;
using SharedApiUtils.Abstractons;

namespace RatingService.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository ratingRepository;
    private readonly ILogger<RatingService> logger;
    private static readonly SemaphoreSlim ratingSemaphore = new SemaphoreSlim(1, 1);

    public RatingService(IRatingRepository ratingRepository, ILogger<RatingService> logger)
    {
        this.ratingRepository = ratingRepository;
        this.logger = logger;
    }
    public async Task<string?> AddSeason(DateOnly beginDate, DateOnly endDate)
    {
        try
        {
            await ratingSemaphore.WaitAsync();
            try
            {
                SeasonEntity seasonEntity = new SeasonEntity()
                {
                    DateStart = beginDate,
                    DateEnd = endDate,
                };
                await ratingRepository.AddSeason(seasonEntity);
                return null;
            }
            finally
            {
                ratingSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while adding a new season");
            return ErrorMessages.InternalServerError;
        }
    }
    public async Task<SeasonModel?> GetCurrentSeason()
    {
        try
        {
            await ratingSemaphore.WaitAsync();
            try
            {
                DateTime now = DateTime.Now;
                logger.LogInformation($"Getting the current season for the date {now.ToShortDateString()}");
                DateOnly currentDate = DateOnly.FromDateTime(now);
                var seasonEntity = await ratingRepository.GetSeasonForDate(currentDate);
                if (seasonEntity is null)
                    return null;

                return new SeasonModel()
                {
                    BeginDate = seasonEntity.DateStart,
                    EndDate = seasonEntity.DateEnd,
                    SeasonId = seasonEntity.Id
                };
            }
            finally
            {
                ratingSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while retrieving the current season");
            return null;
        }
    }

    public async Task<string?> AddOrUpdateUserScore(int seasonId, string userId, int score)
    {
        try
        {
            await ratingSemaphore.WaitAsync();
            try
            {
                logger.LogInformation($"Adding or updating user score. UserId: {userId} SeasonId: {seasonId} Score: {score}");
                var season = await ratingRepository.GetSeasonById(seasonId);
                if (season is null)
                    return ErrorMessages.SeasonNotFound;

                var userScore = await ratingRepository.GetUserScore(userId, seasonId);
                if (score < 0)
                    score = 0;
                if (userScore is null)
                {
                    UserScoreEntity userScoreEntity = new UserScoreEntity()
                    {
                        UserId = userId,
                        Score = score,
                        SeasonId = seasonId,
                    };
                    await ratingRepository.AddUserScore(userScoreEntity);
                    logger.LogInformation($"Added new userscore to database. UserId: {userId} Score: {score}");
                }
                else
                {
                    await ratingRepository.UpdateUserScore(userId, seasonId, score);
                    logger.LogInformation($"Existing user {userId} rating score updated");
                }
                return null;
            }
            finally
            {
                ratingSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while adding or changing the user {userId} score for the season {seasonId}");
            return ErrorMessages.InternalServerError;
        }
    }
    public async Task<int?> GetUserScore(string userId, int seasonId)
    {
        try
        {
            await ratingSemaphore.WaitAsync();
            try
            {
                var userScore = await ratingRepository.GetUserScore(userId, seasonId);
                if (userScore is null)
                    return null;

                return userScore.Score;
            }
            finally
            {
                ratingSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while retrieving the user {userId} score for the season {seasonId}");
            return null;
        }
    }
    public async Task<List<UserScoreModel>> GetRatingList(int seasonId, int SelectedRecords = 10)
    {
        try
        {
            await ratingSemaphore.WaitAsync();
            try
            {
                var rawRatingList = await ratingRepository.GetUserScoresList(seasonId, SelectedRecords);
                var ratingList = rawRatingList.Select(x =>
                {
                    return new UserScoreModel()
                    {
                        Score = x.Score,
                        UserId = x.UserId,
                    };
                }).ToList();
                return ratingList;
            }
            finally
            {
                ratingSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while retrieving the rating list");
            return new List<UserScoreModel>();
        }
    }
    public async Task<List<SeasonModel>> GetAllSeasons()
    {
        try
        {
            await ratingSemaphore.WaitAsync();
            try
            {
                var rawSeasons = await ratingRepository.GetAllSeasons();
                var seasons = rawSeasons.Select(x => new SeasonModel()
                {
                    BeginDate = x.DateStart,
                    EndDate = x.DateEnd,
                    SeasonId = x.Id,
                }).ToList();
                return seasons;
            }
            finally
            {
                ratingSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while retrieving the list of all seasons");
            return new List<SeasonModel>();
        }
    }
}
