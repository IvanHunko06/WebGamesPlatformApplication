using Microsoft.Extensions.Logging;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.RabbitMq.Core.Messages.GameProccessingService;

namespace SharedApiUtils.RabbitMq.Clients;

public class RabbitMqGameProccessingClient : RabbitMqBaseClient, IGameProcessingServiceClient
{
    private readonly ILogger<RabbitMqGameProccessingClient> logger;

    public RabbitMqGameProccessingClient(
        ILogger<RabbitMqGameProccessingClient> logger,
        RabbitMqConnection _connection,
        ILogger<RabbitMqBaseClient> _logger) : base(_connection, _logger)
    {
        this.logger = logger;
    }

    public async Task<(bool IsOver, Dictionary<string, int>? PlayerScores)> CheckGameOver(string gameId, string sessionState)
    {
        try
        {
            var request = new CheckGameOverRequest { SessionState = sessionState };
            var reply = await SendRequest<CheckGameOverRequest, CheckGameOverReply>(request, RabbitMqEvents.CheckGameOver, gameId, ServicesExchanges.GameProccesingExchange);
            return (reply.IsOver, reply.PlayerScores);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to check game win.");
            return (false, null);
        }
    }

    public async Task<string> GetEmptySessionState(string gameId, List<string> players)
    {
        try
        {
            var request = new GetEmptySessionStateRequest { Players = players };
            var reply = await SendRequest<GetEmptySessionStateRequest, GetEmptySessionStateReply>(request, RabbitMqEvents.GetEmptySessionState, gameId, ServicesExchanges.GameProccesingExchange);
            return reply.SessionState;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to get empty session state.");
            return ErrorMessages.InternalServerError;
        }
    }

    public async Task<string> GetGameStateForPlayer(string gameId, string userId, string sessionState)
    {
        try
        {
            var request = new GetGameStateForPlayerRequest { GameState = sessionState, UserId = userId };
            var reply = await SendRequest<GetGameStateForPlayerRequest, GetGameStateForPlayerReply>(request, RabbitMqEvents.GetGameStateForPlayer, gameId, ServicesExchanges.GameProccesingExchange);
            return reply.GameState;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to get empty session state.");
            return ErrorMessages.InternalServerError;
        }
    }

    public async Task<(string? notifyRoomMessage, Dictionary<string, string>? notifyPlayers)> GetSessionDeltaMessages(string gameId, string oldSessionState, string newSessionState)
    {
        try
        {
            var request = new GetSessionDeltaMessagesRequest { OldSessionState = oldSessionState, NewSessionState = newSessionState };
            var reply = await SendRequest<GetSessionDeltaMessagesRequest, GetSessionDeltaMessagesReply>(request, RabbitMqEvents.GetSessionDeltaMessages, gameId, ServicesExchanges.GameProccesingExchange);
            return (reply.NotifyRoomMessage, reply.NotifyPlayersMessages);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to get session delta messages.");
            return (null, null);
        }
    }

    public async Task<(string newSessionState, string? gameErrorMessage)> ProccessAction(string gameId, string sessionState, string userId, string action, string payload)
    {
        try
        {
            var request = new ProccessActionRequest { Action = action, Payload = payload, SessionState = sessionState, UserId = userId };
            var reply = await SendRequest<ProccessActionRequest, ProccessActionReply>(request, RabbitMqEvents.ProccessAction, gameId, ServicesExchanges.GameProccesingExchange);
            return (reply.NewSessionState, reply.GameErrorMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to proccess game action.");
            return (sessionState, null);
        }
    }
}
