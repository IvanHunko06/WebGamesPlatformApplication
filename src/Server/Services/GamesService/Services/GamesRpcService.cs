using GamesService.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;
using SharedApiUtils.Abstractons;
using System.Data;
using GamesService.Interfaces;
namespace GamesService.Services;

public class GamesRpcService : Games.GamesBase
{
    private readonly IGamesService gamesService;

    public GamesRpcService(IGamesService gamesService)
    {
        this.gamesService = gamesService;
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<AddGameReply> AddGame(GameInfo request, ServerCallContext context)
    {
        AddGameReply addGameReply = new AddGameReply();
        GameInfoModel gameInfo = new GameInfoModel()
        {
            GameId = request.GameId,
            ImageUrl = request.ImageUrl,
            LocalizationKey = request.LocalizationKey,
            MaxPlayersCount = request.MaxPlayersCount,
            MinPlayersCount = request.MinPlayersCount,
            StaticPlayersCount = request.StaticPlayersCount,
            SupportSinglePlayer = request.SupportSinglePlayer,
        };
        string? errorMessage = await gamesService.AddGame(gameInfo);
        if(errorMessage is null)
            addGameReply.IsSuccess = true;
        else
            addGameReply.ErrorMessage = errorMessage;
        return addGameReply;
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<DeleteGameReply> DeleteGame(DeleteGameRequest request, ServerCallContext context)
    {

        DeleteGameReply deleteGameReply = new DeleteGameReply();

        string? errorMessage = await gamesService.DeleteGame(request.GameId);
        if (errorMessage is null)
            deleteGameReply.IsSuccess = true;
        else
            deleteGameReply.ErrorMessage = errorMessage;
        return deleteGameReply;
    }


    [Authorize(Policy = "AdminOrPrivateClient")]
    public override async Task<UpdateGameReply> UpdateGame(GameInfo request, ServerCallContext context)
    {
        UpdateGameReply updateGameReply = new UpdateGameReply();
        GameInfoModel gameInfo = new GameInfoModel()
        {
            GameId = request.GameId,
            ImageUrl = request.ImageUrl,
            LocalizationKey = request.LocalizationKey,
            MaxPlayersCount = request.MaxPlayersCount,
            MinPlayersCount = request.MinPlayersCount,
            StaticPlayersCount = request.StaticPlayersCount,
            SupportSinglePlayer = request.SupportSinglePlayer,
        };
        string? errorMessage = await gamesService.UpdateGame(gameInfo);
        if(errorMessage is null)
            updateGameReply.IsSuccess = true;
        else
            updateGameReply.ErrorMessage = errorMessage;
        return updateGameReply;
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    public override async Task<GetGamesListReply> GetGamesList(Empty request, ServerCallContext context)
    {
        GetGamesListReply reply = new GetGamesListReply();
        var gamesList = await gamesService.GetGamesList();
        if(gamesList is null)
            return reply;
        var gameInfoList = gamesList.Select(g =>
        {
            return new GameInfo()
            {
                GameId = g.GameId,
                ImageUrl = g.ImageUrl,
                LocalizationKey = g.LocalizationKey,
                MaxPlayersCount = g.MaxPlayersCount,
                MinPlayersCount = g.MinPlayersCount,
                StaticPlayersCount = g.StaticPlayersCount,
                SupportSinglePlayer = g.SupportSinglePlayer,
            };
        });
        reply.Games.AddRange(gameInfoList);
        return reply;
    }
}
