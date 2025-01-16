using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.RabbitMq.Core.Messages.GameSessionWsNotifyer;

namespace SharedApiUtils.RabbitMq.Clients;

public class RabbitMqGameSessionWsNotifyerClient : IGameSessionWsNotifyerClient
{
    private readonly RabbitMqConnection connection;
    private readonly ILogger<RabbitMqGameSessionWsNotifyerClient> logger;
    private static IChannel? channel = null;

    public RabbitMqGameSessionWsNotifyerClient(RabbitMqConnection connection, ILogger<RabbitMqGameSessionWsNotifyerClient> logger)
    {
        this.connection = connection;
        this.logger = logger;
    }
    public async Task NotifyReciveAction_AllUsers(string sessionId, string message)
    {
        try
        {
            channel ??= await connection.GetNewChannel();
            await channel.QueueDeclareAsync(ServicesQueues.GameSessionWsNotifyer, true, false, false);
            BasicProperties properties = new BasicProperties();
            properties.Headers = new Dictionary<string, object?>();
            properties.Headers[WellKnownHeaders.EventType] = RabbitMqEvents.NotifyReciveAction_AllUsers;
            properties.Headers[WellKnownHeaders.BodyType] = nameof(NotifyReciveActionAllUsersRequest);
            NotifyReciveActionAllUsersRequest request = new NotifyReciveActionAllUsersRequest()
            {
                Message = message,
                SessionId = sessionId
            };
            var body = JsonSerializer.SerializeToUtf8Bytes(request);
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: ServicesQueues.GameSessionWsNotifyer,
                body: body,
                basicProperties: properties,
                mandatory: false);
            logger.LogDebug($"Message sended to {ServicesQueues.GameSessionWsNotifyer} queue. Body: {JsonSerializer.Serialize(request)}. Event type: {RabbitMqEvents.NotifyReciveAction_AllUsers}. Channel number: {channel.ChannelNumber}");
            logger.LogInformation($"message NotifyReciveAction_AllUsers sent to {ServicesQueues.GameSessionWsNotifyer} queue");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while sending the NotifyReciveAction_AllUsers message");
        }
    }

    public async Task NotifyReciveAction_User(string sessionId, string userId, string message)
    {
        try
        {
            channel ??= await connection.GetNewChannel();
            await channel.QueueDeclareAsync(ServicesQueues.GameSessionWsNotifyer, true, false, false);
            BasicProperties properties = new BasicProperties();
            properties.Headers = new Dictionary<string, object?>();
            properties.Headers[WellKnownHeaders.EventType] = RabbitMqEvents.NotifyReciveAction_User;
            properties.Headers[WellKnownHeaders.BodyType] = nameof(NotifyReciveActionUserRequest);
            NotifyReciveActionUserRequest request = new NotifyReciveActionUserRequest()
            {
                Message = message,
                SessionId = sessionId,
                UserId = userId,
            };
            var body = JsonSerializer.SerializeToUtf8Bytes(request);
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: ServicesQueues.GameSessionWsNotifyer,
                body: body,
                basicProperties: properties,
                mandatory: false);
            logger.LogDebug($"Message sended to {ServicesQueues.GameSessionWsNotifyer} queue. Body: {JsonSerializer.Serialize(request)}. Event type: {RabbitMqEvents.NotifyReciveAction_User}. Channel number: {channel.ChannelNumber}");
            logger.LogInformation($"message NotifyReciveAction_Users sent to {ServicesQueues.GameSessionWsNotifyer} queue");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while sending the NotifyReciveAction_Users message");
        }
    }

    public async Task NotifySessionEnded_User(string sessionId, string userId, string endReason, string? payload)
    {
        try
        {
            channel ??= await connection.GetNewChannel();
            await channel.QueueDeclareAsync(ServicesQueues.GameSessionWsNotifyer, true, false, false);
            BasicProperties properties = new BasicProperties();
            properties.Headers = new Dictionary<string, object?>();
            properties.Headers[WellKnownHeaders.EventType] = RabbitMqEvents.NotifySessionEnded_User;
            properties.Headers[WellKnownHeaders.BodyType] = nameof(NotifySessionEndedUserRequest);
            var request = new NotifySessionEndedUserRequest()
            {
               SessionId = sessionId,
               UserId = userId,
               Payload = payload,
               Reason = endReason,
            };
            var body = JsonSerializer.SerializeToUtf8Bytes(request);
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: ServicesQueues.GameSessionWsNotifyer,
                body: body,
                basicProperties: properties,
                mandatory: false);
            logger.LogDebug($"Message sended to {ServicesQueues.GameSessionWsNotifyer} queue. Body: {JsonSerializer.Serialize(request)}. Event type: {RabbitMqEvents.NotifySessionEnded_User}. Channel number: {channel.ChannelNumber}");
            logger.LogInformation($"message {RabbitMqEvents.NotifySessionEnded_User} sent to {ServicesQueues.GameSessionWsNotifyer} queue");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"an error occurred while sending the {RabbitMqEvents.NotifySessionEnded_User} message");
        }
    }

    public async Task NotifySessionEnded_AllUser(string sessionId, string endReason, string? payload)
    {
        try
        {
            channel ??= await connection.GetNewChannel();
            await channel.QueueDeclareAsync(ServicesQueues.GameSessionWsNotifyer, true, false, false);
            BasicProperties properties = new BasicProperties();
            properties.Headers = new Dictionary<string, object?>();
            properties.Headers[WellKnownHeaders.EventType] = RabbitMqEvents.NotifySessionEnded_AllUsers;
            properties.Headers[WellKnownHeaders.BodyType] = nameof(NotifySessionEndedAllUsersRequest);
            var request = new NotifySessionEndedAllUsersRequest()
            {
                SessionId = sessionId,
                Payload = payload,
                Reason = endReason,
            };
            var body = JsonSerializer.SerializeToUtf8Bytes(request);
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: ServicesQueues.GameSessionWsNotifyer,
                body: body,
                basicProperties: properties,
                mandatory: false);
            logger.LogDebug($"Message sended to {ServicesQueues.GameSessionWsNotifyer} queue. Body: {JsonSerializer.Serialize(request)}. Event type: {RabbitMqEvents.NotifySessionEnded_AllUsers}. Channel number: {channel.ChannelNumber}");
            logger.LogInformation($"message {RabbitMqEvents.NotifySessionEnded_AllUsers} sent to {ServicesQueues.GameSessionWsNotifyer} queue");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"an error occurred while sending the {RabbitMqEvents.NotifySessionEnded_AllUsers} message");
        }
    }
}
