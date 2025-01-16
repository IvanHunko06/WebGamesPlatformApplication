using Azure.Core;
using GamesService.Interfaces;
using GamesService.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedApiUtils.Abstractons;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;

namespace GamesService.Services;

public class GamesService : IGamesService
{
    private readonly IGamesRepository gamesRepository;
    private readonly ILogger<GamesService> logger;

    public GamesService(IGamesRepository gamesRepository, ILogger<GamesService> logger)
    {
        this.gamesRepository = gamesRepository;
        this.logger = logger;
    }
    public async Task<string?> AddGame(GameInfoModel gameDto)
    {
        try
        {
            GameInfoEntity gameInfoEntity = new GameInfoEntity()
            {
                GameId = gameDto.GameId,
                ImageUrl = gameDto.ImageUrl,
                LocalizationKey = gameDto.LocalizationKey,
                MaxPlayersCount = gameDto.MaxPlayersCount,
                MinPlayersCount = gameDto.MinPlayersCount,
                StaticPlayersCount = gameDto.StaticPlayersCount,
                SupportSinglePlayer = gameDto.SupportSinglePlayer,
            };
            if (gameDto.StaticPlayersCount) gameInfoEntity.MinPlayersCount = gameInfoEntity.MaxPlayersCount;
            await gamesRepository.AddGame(gameInfoEntity);
            return null;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException)
        {
            var exception = (SqlException)ex.InnerException;
            if (exception.Number == 2601 || exception.Number == 2627)
            {
                return ErrorMessages.DublicateGameId;
            }
            else if (exception.Number == 2628)
            {
                return ErrorMessages.PropertyTooLong;
            }
            else
            {
                return ErrorMessages.InternalServerError;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while adding the game");
            return ErrorMessages.InternalServerError;
        }
    }
    public async Task<string?> DeleteGame(string gameId)
    {
        try
        {
            await gamesRepository.DeleteGame(gameId);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while deleting the game");
            return ErrorMessages.InternalServerError;
        }
    }
    public async Task<List<GameInfoModel>?> GetGamesList()
    {
        try
        {
            var games = (await gamesRepository.GetAllGames()).Select(g =>
            {
                return new GameInfoModel()
                {
                    GameId = g.GameId,
                    ImageUrl = g.ImageUrl,
                    LocalizationKey = g.LocalizationKey,
                    MaxPlayersCount = g.MaxPlayersCount,
                    MinPlayersCount = g.MinPlayersCount,
                    StaticPlayersCount = g.StaticPlayersCount,
                    SupportSinglePlayer = g.SupportSinglePlayer,
                };
            }).ToList();
            return games;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while getting the list of games");
            return null;
        }
    }
    public async Task<string?> UpdateGame(GameInfoModel gameInfo)
    {
        try
        {
            await gamesRepository.UpdateGame(new GameInfoEntity()
            {
                GameId = gameInfo.GameId,
                ImageUrl = gameInfo.ImageUrl,
                LocalizationKey = gameInfo.LocalizationKey,
                MaxPlayersCount = gameInfo.MaxPlayersCount,
                MinPlayersCount = gameInfo.MinPlayersCount,
                StaticPlayersCount = gameInfo.StaticPlayersCount,
                SupportSinglePlayer = gameInfo.SupportSinglePlayer,
            });
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while updating the game");
            return ErrorMessages.InternalServerError;
        }
    }
}
