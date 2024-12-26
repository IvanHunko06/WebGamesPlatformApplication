using SharedApiUtils.Interfaces;
using SharedApiUtils.ServicesAccessing.Connections;
using SharedApiUtils.ServicesAccessing.Protos;
using System;

namespace SharedApiUtils.ServicesClients;

public class GameSessionServiceClient : IGameSessionServiceClient
{
    private readonly GameSessionConnection gameSessionConnection;

    public GameSessionServiceClient(GameSessionConnection gameSessionConnection)
    {
        this.gameSessionConnection = gameSessionConnection;
    }
    public async Task<StartGameSessionReply?> StartGameSession(string roomId, IEnumerable<string> members)
    {
        try
        {
            var combinationClient = await gameSessionConnection.GetClient();
            if (combinationClient.client is null || combinationClient.headers is null)
                return null;
            StartGameSessionRequest request = new StartGameSessionRequest()
            {
                RoomId = roomId,
            };
            request.Members.AddRange(members);
            var response = await combinationClient.client.StartGameSessionAsync(request, combinationClient.headers);
            return response;
        }
        catch (Exception)
        {
            throw;
        }
        
    }
    public async Task<GetGameSessionReply?> GetGameSession(string sessionId)
    {
        try
        {
            var combinationClient = await gameSessionConnection.GetClient();
            if (combinationClient.client is null || combinationClient.headers is null)
                return null;
            GetGameSessionRequest request = new GetGameSessionRequest()
            {
                SessionId = sessionId,
            };
            var response = await combinationClient.client.GetGameSessionAsync(request, combinationClient.headers);
            return response;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<SendGameEventReply?> SendGameEvent(string playerId, string sessionId, string action, string payload)
    {
        try
        {
            var combinationClient = await gameSessionConnection.GetClient();
            if (combinationClient.client is null || combinationClient.headers is null)
                return null;
            SendGameEventRequest request = new SendGameEventRequest()
            {
                Action = action,
                Payload = payload,
                SessionId = sessionId,
                UserId = playerId
            };
            var response = await combinationClient.client.SendGameEventAsync(request, combinationClient.headers);
            return response;
        }
        catch (Exception)
        {
            throw;
        }
        
    }
    public async Task<EndGameSessionReply?> EndGameSession(string sessionId, string reason, string? payload)
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
            return response;
        }
        catch (Exception)
        {
            throw;
        }
        
    }
}
