using Microsoft.Extensions.Logging;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.RabbitMq.Core.Messages.GameProccessingService;
using SharedApiUtils.RabbitMq.Core.Messages.RoomsService;

namespace SharedApiUtils.RabbitMq.Clients;

public class RabbitMqGameProccessingClient : RabbitMqBaseClient, IGameProcessingServiceClient
{
    private readonly ILogger<RabbitMqGameProccessingClient> logger;

    public RabbitMqGameProccessingClient(
        ILogger<RabbitMqGameProccessingClient> logger,
        RabbitMqConnection _connection) : base(_connection)
    {
        this.logger = logger;
    }

    public async Task<string> GetEmptySessionState(string gameId, IEnumerable<string> players)
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

    public async Task<(string newSessionState, string? gameErrorMessage)> ProccessAction(string gameId, string sessionState, string userId, string action, string payload)
    {
        try
        {
            var request = new ProccessActionRequest {Action = action, Payload = payload, SessionState = sessionState, UserId = userId };
            var reply = await SendRequest<ProccessActionRequest, ProccessActionReply>(request, RabbitMqEvents.GetEmptySessionState, gameId, ServicesExchanges.GameProccesingExchange);
            return (reply.NewSessionState, reply.GameErrorMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to add a user to the room.");
            return (sessionState, null);
        }
    }
}
