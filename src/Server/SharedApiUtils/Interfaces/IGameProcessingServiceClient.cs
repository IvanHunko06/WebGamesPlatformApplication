using SharedApiUtils.ServicesAccessing.Protos;

namespace SharedApiUtils.Interfaces;

public interface IGameProcessingServiceClient
{
    Task<string?> GetEmptySessionState(string gameLogicServiceUrl, IEnumerable<string> players);
    Task<ProccessActionReply?> ProccessAction(string gameLogicServiceUrl, string sessionState, string userId, string action, string payload);
}