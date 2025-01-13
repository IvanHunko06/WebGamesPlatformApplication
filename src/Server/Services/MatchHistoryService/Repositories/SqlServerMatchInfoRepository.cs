using MatchHistoryService.Entities;
using MatchHistoryService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MatchHistoryService.Repositories;

public class SqlServerMatchInfoRepository : IMatchInfoRepository
{
    private readonly MatchHistoryDbContext dbContext;

    public SqlServerMatchInfoRepository(MatchHistoryDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    public async Task AddMatchInfo(MatchInformationEntity matchInformation)
    {
        try
        {
            dbContext.MatchInformations.Add(matchInformation);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task DeleteMatchInfo(Guid recordId)
    {
        try
        {
            await dbContext.MatchInformations
            .Where(x => x.RecordId == recordId)
            .ExecuteDeleteAsync();

        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<MatchInformationEntity>> GetAll()
    {
        try
        {
            var matches = await dbContext.MatchInformations.Include(x => x.UserScores).ToListAsync();
            return matches;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<MatchInformationEntity>> GetAllForUser(string userId)
    {
        try
        {
            var matches = await dbContext.MatchInformations
            .Include(x => x.UserScores)
            .Where(x => x.UserScores.Any(x => x.UserId == userId))
            .ToListAsync();
            return matches;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
