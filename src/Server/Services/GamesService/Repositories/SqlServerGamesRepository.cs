using Azure.Core;
using GamesService.Interfaces;
using GamesService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedApiUtils.Abstractons;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;

namespace GamesService.Repositories;

public class SqlServerGamesRepository : IGamesRepository
{
    private readonly GamesServerDbContext dbContext;

    public SqlServerGamesRepository(GamesServerDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    public async Task AddGame(GameInfoEntity gameInfo)
    {
        try
        {
            dbContext.GameInfos.Add(gameInfo);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task DeleteGame(string gameId)
    {
        try
        {
            await dbContext.GameInfos.Where(g => g.GameId == gameId).ExecuteDeleteAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<List<GameInfoEntity>> GetAllGames()
    {
        try
        {
            var list = await dbContext.GameInfos.ToListAsync();
            return list;
        }
        catch(Exception)
        {
            throw;
        }
    }

    public async Task<GameInfoEntity?> GetGameById(string gameId)
    {
        try
        {
            var game = await dbContext.GameInfos.Where(g => g.GameId == gameId).FirstOrDefaultAsync();
            return game;

        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task UpdateGame(GameInfoEntity gameInfo)
    {
        try
        {
            await dbContext.GameInfos.Where(g => g.GameId == gameInfo.GameId)
                .ExecuteUpdateAsync(g => g
                .SetProperty(g => g.ImageUrl, gameInfo.ImageUrl)
                .SetProperty(g => g.LocalizationKey, gameInfo.LocalizationKey)
                .SetProperty(g => g.MaxPlayersCount, gameInfo.MaxPlayersCount)
                .SetProperty(g => g.MinPlayersCount, gameInfo.MinPlayersCount)
                .SetProperty(g => g.StaticPlayersCount, gameInfo.StaticPlayersCount)
                .SetProperty(g => g.SupportSinglePlayer, gameInfo.SupportSinglePlayer)
                );
        }
        catch (Exception)
        {
            throw;
        }
    }
}
