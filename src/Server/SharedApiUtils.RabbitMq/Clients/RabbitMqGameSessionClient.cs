using Microsoft.Extensions.Logging;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Core;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.RabbitMq.Core.Messages.GameSessionService;

namespace SharedApiUtils.RabbitMq.Clients;

public class RabbitMqGameSessionClient : RabbitMqBaseClient, IGameSessionServiceClient
{
    private readonly ILogger<RabbitMqGameSessionClient> logger;
    public RabbitMqGameSessionClient(
        ILogger<RabbitMqGameSessionClient> logger,
        RabbitMqConnection _connection,
        RabbitMqMessagePublisher _messagePublisher) : base(_connection, _messagePublisher)
    {
        this.logger = logger;
    }

    public async Task<string?> EndGameSession(string sessionId, string reason, string? payload)
    {
        try
        {
            var request = new EndGameSessionRequest { SessionId = sessionId, Reason = reason, Payload = payload };
            var reply = await SendRequest<EndGameSessionRequest, EndGameSessionReply>(request, RabbitMqEvents.EndGameSession, ServicesQueues.GameSessionServiceQueue);
            return reply.IsSuccess ? null : reply.ErrorMessage;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to end the session");
            return ErrorMessages.InternalServerError;
        }
    }

    public async Task<(string? errorMessage, GameSessionDto? gameSession)> GetGameSession(string sessionId)
    {
        try
        {
            var request = new GetGameSessionRequest { SessionId = sessionId };
            var reply = await SendRequest<GetGameSessionRequest, GetGameSessionReply>(request, RabbitMqEvents.GetGameSession, ServicesQueues.GameSessionServiceQueue);
            return reply.IsSuccess ? (null, reply.GameSession) : (reply.ErrorMessage, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to get game session.");
            return (ErrorMessages.InternalServerError, null);
        }
    }

    public async Task<(string? errorMessage, string? gameErrorMessage)> SendGameEvent(string playerId, string sessionId, string action, string payload)
    {
        try
        {
            var request = new SendGameEventRequest { SessionId = sessionId, PlayerId = playerId, Action = action, Payload = payload };
            var reply = await SendRequest<SendGameEventRequest, SendGameEventReply>(request, RabbitMqEvents.SendGameEvent, ServicesQueues.GameSessionServiceQueue);
            return (reply.ErrorMessage, reply.GameErrorMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to send game event");
            return (ErrorMessages.InternalServerError, null);
        }
    }

    public async Task<(string? sessionId, string? errorMessage)> StartGameSession(string roomId)
    {
        try
        {
            var request = new StartGameSessionRequest { RoomId = roomId };
            var reply = await SendRequest<StartGameSessionRequest, StartGameSessionReply>(request, RabbitMqEvents.StartGameSession, ServicesQueues.GameSessionServiceQueue);
            return reply.IsSuccess ? (reply.SessionId, null) : (null, reply.ErrorMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to start game session.");
            return (null, ErrorMessages.InternalServerError);
        }
    }

    public async Task<(string? errorMessage, string? gameState)> SyncGameState(string playerId, string sessionId)
    {
        try
        {
            var request = new SyncGameStateRequest { SessionId = sessionId, PlayerId = playerId };
            var reply = await SendRequest<SyncGameStateRequest, SyncGameStateReply>(request, RabbitMqEvents.SyncGameState, ServicesQueues.GameSessionServiceQueue);
            return reply.IsSuccess ? (null, reply.GameState) : (reply.ErrorMessage, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to sync game state.");
            return (ErrorMessages.InternalServerError, null);
        }
    }
}
