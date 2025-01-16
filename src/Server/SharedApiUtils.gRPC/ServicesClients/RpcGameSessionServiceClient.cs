using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Core;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.gRPC.ServicesAccessing.Connections;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;

namespace SharedApiUtils.gRPC.ServicesClients;

public class RpcGameSessionServiceClient : IGameSessionServiceClient
{
    private readonly GameSessionConnection gameSessionConnection;

    public RpcGameSessionServiceClient(GameSessionConnection gameSessionConnection)
    {
        this.gameSessionConnection = gameSessionConnection;
    }

    public async Task<(string? errorMessage, string? gameErrorMessage)> SendGameEvent(string playerId, string sessionId, string action, string payload)
    {
        throw new NotImplementedException();
        //try
        //{
        //    var combinationClient = await gameSessionConnection.GetClient();
        //    if (combinationClient.client is null || combinationClient.headers is null)
        //        return null;
        //    SendGameEventRequest request = new SendGameEventRequest()
        //    {
        //        Action = action,
        //        Payload = payload,
        //        SessionId = sessionId,
        //        UserId = playerId
        //    };
        //    var response = await combinationClient.client.SendGameEventAsync(request, combinationClient.headers);
        //    if (response.IsSuccess)
        //        return null;
        //    else
        //        return response.ErrorMessage;
        //}
        //catch (Exception)
        //{
        //    throw;
        //}
    }

    public async Task<string?> EndGameSession(string sessionId, string reason, string? payload)
    {
        try
        {
            var combinationClient = await gameSessionConnection.GetClient();
            if (combinationClient.client is null || combinationClient.headers is null)
                return null;
            EndGameSessionRequest request = new EndGameSessionRequest()
            {
                SessionId = sessionId,
                EndReason = reason,
                Payload = payload
            };
            var response = await combinationClient.client.EndGameSessionAsync(request, combinationClient.headers);
            if (response.IsSuccess)
                return null;
            else
                return response.ErrorMessage;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<(string? sessionId, string? errorMessage)> StartGameSession(string roomId)
    {
        try
        {
            var combinationClient = await gameSessionConnection.GetClient();
            if (combinationClient.client is null || combinationClient.headers is null)
                return (null, ErrorMessages.InternalServerError);
            StartGameSessionRequest request = new StartGameSessionRequest()
            {
                RoomId = roomId,
            };
            var response = await combinationClient.client.StartGameSessionAsync(request, combinationClient.headers);
            if (response is null) return (null, ErrorMessages.InternalServerError);
            if (!response.IsSuccess)
                return (null, response.ErrorMessage);
            return (response.SessionId, null);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<(string? errorMessage, GameSessionDto? gameSession)> GetGameSession(string sessionId)
    {
        try
        {
            var combinationClient = await gameSessionConnection.GetClient();
            if (combinationClient.client is null || combinationClient.headers is null)
                return (ErrorMessages.InternalServerError, null);
            GetGameSessionRequest request = new GetGameSessionRequest()
            {
                SessionId = sessionId,
            };
            var response = await combinationClient.client.GetGameSessionAsync(request, combinationClient.headers);
            if (response is null) return (ErrorMessages.InternalServerError, null);
            if(!response.IsSuccess)
                return (response.ErrorMessage, null);

            return (null, new GameSessionDto()
            {
                RoomId = response.GameSession.RoomId,
                EndTime = response.GameSession.EndTime is not null ? DateTimeOffset.Parse(response.GameSession.EndTime) : null,
                StartTime = DateTimeOffset.Parse(response.GameSession.StartTime),
                GameId = response.GameSession.GameId,
                SessionId = response.GameSession.SessionId,
                SessionState = response.GameSession.SessionState,
                PlayerScores = response.GameSession.PlayerScores.Select(s =>
                {
                    return new KeyValuePair<string, int>(s.Key, s.Value);
                }).ToDictionary(),
                OwnerId = response.GameSession.OwnerId,
                Players = new List<string>(response.GameSession.Players)
            });

        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task<(string? errorMessage, string? gameState)> SyncGameState(string playerId, string sessionId)
    {
        throw new NotImplementedException();
    }
}
