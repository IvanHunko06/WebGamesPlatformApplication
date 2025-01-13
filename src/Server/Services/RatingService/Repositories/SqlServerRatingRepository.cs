using Microsoft.EntityFrameworkCore;
using RatingService.Entities;
using RatingService.Interfaces;

namespace RatingService.Repositories;

public class SqlServerRatingRepository : IRatingRepository
{
    private readonly RatingDbContext dbContext;

    public SqlServerRatingRepository(RatingDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    public async Task AddSeason(SeasonEntity season)
    {
        try
        {
            dbContext.Seasons.Add(season);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<SeasonEntity?> GetSeasonForDate(DateOnly date)
    {
        try
        {
            var season = await dbContext.Seasons
            .AsNoTracking()
            .Where(season => date >= season.DateStart && date <= season.DateEnd)
            .OrderByDescending(x => x.DateEnd)
            .FirstOrDefaultAsync();
            return season;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<SeasonEntity>> GetAllSeasons()
    {
        try
        {
            var seasons = await dbContext.Seasons
            .OrderByDescending(x => x.DateEnd)
            .ToListAsync();
            return seasons;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<SeasonEntity?> GetSeasonById(int seasonId)
    {
        try
        {
            var season = await dbContext.Seasons
            .AsNoTracking()
            .Where(x => x.Id == seasonId)
            .FirstOrDefaultAsync();
            return season;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task AddUserScore(UserScoreEntity userScore)
    {
        try
        {
            dbContext.UserScores.Add(userScore);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task UpdateUserScore(string userId, int seasonId, int newScore)
    {
        try
        {
            await dbContext.UserScores
            .Where(x => x.UserId == userId && x.SeasonId == seasonId)
            .ExecuteUpdateAsync(x => x
            .SetProperty(y => y.Score, newScore));
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<UserScoreEntity?> GetUserScore(string userId, int seasonId)
    {
        try
        {
            var userScore = await dbContext.UserScores
            .Where(x => x.UserId == userId && x.SeasonId == seasonId)
            .FirstOrDefaultAsync();

            return userScore;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<UserScoreEntity>> GetUserScoresList(int seasonId, int selectedRecords)
    {
        try
        {
            var userScores = await dbContext.UserScores
            .Where(x => x.SeasonId == seasonId)
            .OrderBy(x => x.Score)
            .Take(selectedRecords)
            .ToListAsync();
            return userScores;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
