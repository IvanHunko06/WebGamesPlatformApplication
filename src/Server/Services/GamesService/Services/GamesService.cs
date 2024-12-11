using GamesService.Models;
using GamesService.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using SharedApiUtils;
namespace GamesService.Services;

public class GamesService : Games.GamesBase
{
    private readonly ILogger<GamesService> logger;
    private readonly GamesServerDbContext dbContext;

    public GamesService(ILogger<GamesService> logger, GamesServerDbContext dbContext)
    {
        this.logger = logger;
        this.dbContext = dbContext;
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<AddGameReply> AddGame(GameInfo request, ServerCallContext context)
    {
        AddGameReply addGameReply = new AddGameReply();
        try
        {
            GameInfoEntity gameInfoEntity = new GameInfoEntity()
            {
                GameId = request.GameId,
                GameLogicServerUrl = request.GameLogicServerUrl,
                ImageUrl = request.ImageUrl,
                LocalizationKey = request.LocalizationKey,
                MaxPlayersCount = request.MaxPlayersCount,
                MinPlayersCount = request.MinPlayersCount,
                StaticPlayersCount = request.StaticPlayersCount,
                SupportSinglePlayer = request.SupportSinglePlayer,
            };
            if (request.StaticPlayersCount) gameInfoEntity.MinPlayersCount = gameInfoEntity.MaxPlayersCount;
            dbContext.GameInfos.Add(gameInfoEntity);
            await dbContext.SaveChangesAsync();
            addGameReply.IsSuccess = true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException)
        {
            var exception = (SqlException)ex.InnerException;
            if (exception.Number == 2601 || exception.Number == 2627)
            {
                addGameReply.IsSuccess = false;
                addGameReply.ErrorMessage = ErrorMessages.DublicateGameId;
            }
            else if (exception.Number == 2628)
            {
                addGameReply.IsSuccess = false;
                addGameReply.ErrorMessage = ErrorMessages.PropertyTooLong;
            }
            else
            {
                addGameReply.IsSuccess = false;
                addGameReply.ErrorMessage = ErrorMessages.InternalServerError;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            addGameReply.IsSuccess = false;
            addGameReply.ErrorMessage = ErrorMessages.InternalServerError;
        }
        return addGameReply;
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<DeleteGameReply> DeleteGame(DeleteGameRequest request, ServerCallContext context)
    {

        DeleteGameReply deleteGameReply = new DeleteGameReply();
        try
        {
            await dbContext.GameInfos.Where(g => g.GameId == request.GameId).ExecuteDeleteAsync();
            deleteGameReply.IsSuccess = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            deleteGameReply.IsSuccess = false;
            deleteGameReply.ErrorMessage = ErrorMessages.InternalServerError;
        }

        return deleteGameReply;
    }


    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<UpdateGameReply> UpdateGame(GameInfo request, ServerCallContext context)
    {
        UpdateGameReply updateGameReply = new UpdateGameReply();

        try
        {
            await dbContext.GameInfos.Where(g => g.GameId == request.GameId)
                .ExecuteUpdateAsync(g => g
                .SetProperty(p => p.GameLogicServerUrl, request.GameLogicServerUrl)
                .SetProperty(g => g.ImageUrl, request.ImageUrl)
                .SetProperty(g => g.LocalizationKey, request.LocalizationKey)
                .SetProperty(g => g.MaxPlayersCount, request.MaxPlayersCount)
                .SetProperty(g => g.MinPlayersCount, request.MinPlayersCount)
                .SetProperty(g => g.StaticPlayersCount, request.StaticPlayersCount)
                .SetProperty(g => g.SupportSinglePlayer, request.SupportSinglePlayer)
                );
            updateGameReply.IsSuccess = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            updateGameReply.IsSuccess = false;
            updateGameReply.ErrorMessage = ErrorMessages.InternalServerError;
        }
        return updateGameReply;
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<GetGamesListReply> GetGamesList(Empty request, ServerCallContext context)
    {
        GetGamesListReply reply = new GetGamesListReply();

        try
        {
            var gameInfoEntities = await dbContext.GameInfos.AsNoTracking().ToListAsync();
            foreach (var gameInfoEntity in gameInfoEntities)
            {
                reply.Games.Add(new GameInfo()
                {
                    GameId = gameInfoEntity.GameId,
                    GameLogicServerUrl = gameInfoEntity.GameLogicServerUrl,
                    ImageUrl = gameInfoEntity.ImageUrl,
                    LocalizationKey = gameInfoEntity.LocalizationKey,
                    MaxPlayersCount = gameInfoEntity.MaxPlayersCount,
                    MinPlayersCount = gameInfoEntity.MinPlayersCount,
                    StaticPlayersCount = gameInfoEntity.StaticPlayersCount,
                    SupportSinglePlayer = gameInfoEntity.SupportSinglePlayer,
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
        }


        return reply;
    }
}
