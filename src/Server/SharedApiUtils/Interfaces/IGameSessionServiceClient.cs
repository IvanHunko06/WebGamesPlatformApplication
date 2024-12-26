using SharedApiUtils.ServicesAccessing.Protos;

namespace SharedApiUtils.Interfaces;

public interface IGameSessionServiceClient
{
    Task<StartGameSessionReply?> StartGameSession(string roomId, IEnumerable<string> members);
    Task<GetGameSessionReply?> GetGameSession(string sessionId);
    Task<SendGameEventReply?> SendGameEvent(string playerId, string sessionId, string action, string payload);
    Task<EndGameSessionReply?> EndGameSession(string sessionId, string reason, string? payload);
}
