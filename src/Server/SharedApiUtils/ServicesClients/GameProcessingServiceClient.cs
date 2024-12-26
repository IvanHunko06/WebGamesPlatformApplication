using Google.Protobuf.WellKnownTypes;
using SharedApiUtils.Interfaces;
using SharedApiUtils.ServicesAccessing.Connections;
using SharedApiUtils.ServicesAccessing.Protos;

namespace SharedApiUtils.ServicesClients;

public class GameProcessingServiceClient : IGameProcessingServiceClient
{
    private readonly GameProcessingServiceConnection connection;

    public GameProcessingServiceClient(GameProcessingServiceConnection connection)
    {
        this.connection = connection;
    }
    public async Task<string?> GetEmptySessionState(string gameLogicServiceUrl, IEnumerable<string> players)
    {
        try
        {
            var combinedClient = await connection.GetClient(gameLogicServiceUrl, true);
            if (combinedClient.client is null || combinedClient.headers is null)
                return null;
            InitEmptySessionStateRequest request = new InitEmptySessionStateRequest();
            request.Players.AddRange(players);
            var response = await combinedClient.client.InitEmptySessionStateAsync(request, combinedClient.headers);
            if (response is null) return null;
            return response.JsonSessionStateObject;
        }
        catch (Exception)
        {
            throw;
        }

    }
    public async Task<ProccessActionReply?> ProccessAction(string gameLogicServiceUrl, string sessionState, string userId ,string action, string payload)
    {
        try
        {
            var combinedClient = await connection.GetClient(gameLogicServiceUrl, true);
            if (combinedClient.client is null || combinedClient.headers is null)
                return null;
            ProccessActionRequest request = new ProccessActionRequest()
            {
                Action = action,
                JsonSessionStateObject = sessionState,
                Payload = payload,
                PlayerId = userId
            };
            var reply = await combinedClient.client.ProccessActionAsync(request, combinedClient.headers);
            return reply;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
