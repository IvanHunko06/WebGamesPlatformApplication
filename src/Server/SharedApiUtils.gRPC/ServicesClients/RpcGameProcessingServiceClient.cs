using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.gRPC.ServicesAccessing.Connections;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;

namespace SharedApiUtils.gRPC.ServicesClients;

//public class RpcGameProcessingServiceClient : IGameProcessingServiceClient
//{
//    private readonly GameProcessingServiceConnection connection;

//    public RpcGameProcessingServiceClient(GameProcessingServiceConnection connection)
//    {
//        this.connection = connection;
//    }

//    public Task<string> GetEmptySessionState(string gameId, List<string> players)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<string> GetGameStateForPlayer(string gameId, string userId, string sessionState)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<(string newSessionState, string? gameErrorMessage)> ProccessAction(string gameId, string sessionState, string userId, string action, string payload)
//    {
//        throw new NotImplementedException();
//    }
//    //public async Task<string?> GetEmptySessionState(string targetService, IEnumerable<string> players)
//    //{
//    //    try
//    //    {
//    //        var combinedClient = await connection.GetClient(targetService, true);
//    //        if (combinedClient.client is null || combinedClient.headers is null)
//    //            return null;
//    //        InitEmptySessionStateRequest request = new InitEmptySessionStateRequest();
//    //        request.Players.AddRange(players);
//    //        var response = await combinedClient.client.InitEmptySessionStateAsync(request, combinedClient.headers);
//    //        if (response is null) return null;
//    //        return response.JsonSessionStateObject;
//    //    }
//    //    catch (Exception)
//    //    {
//    //        throw;
//    //    }
//    //}
//    //public async Task<string?> ProccessAction(string targetService, string sessionState, string userId, string action, string payload)
//    //{
//    //    try
//    //    {
//    //        var combinedClient = await connection.GetClient(targetService, true);
//    //        if (combinedClient.client is null || combinedClient.headers is null)
//    //            return null;
//    //        ProccessActionRequest request = new ProccessActionRequest()
//    //        {
//    //            Action = action,
//    //            JsonSessionStateObject = sessionState,
//    //            Payload = payload,
//    //            PlayerId = userId
//    //        };
//    //        var reply = await combinedClient.client.ProccessActionAsync(request, combinedClient.headers);
//    //        if (reply.IsSuccess)
//    //            return null;
//    //        else
//    //            return reply.GameErrorMessage;
//    //    }
//    //    catch (Exception)
//    //    {
//    //        throw;
//    //    }
//    //}
//}
