﻿using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SharedApiUtils.RabbitMq.Core.Messages.GameProccessingService;
using System.Text.Json;

namespace SharedApiUtils.RabbitMq.Listeners;

public abstract class BaseRabbitMqGameProccessingListener : BaseRabbitMqMessageListener
{
    private readonly ILogger<BaseRabbitMqGameProccessingListener> logger;
    private readonly RabbitMqConnection connection;

    public BaseRabbitMqGameProccessingListener(
        ILogger<BaseRabbitMqGameProccessingListener> logger,
        RabbitMqConnection connection,
        ILogger<BaseRabbitMqMessageListener> _logger
        ) : base(connection, _logger)
    {
        this.logger = logger;
        this.connection = connection;
    }
    public async Task StartListening(string gameId)
    {

        var channel = connection.GetChannel();
        await channel.ExchangeDeclareAsync(ServicesExchanges.GameProccesingExchange, ExchangeType.Direct, true);
        await channel.QueueDeclareAsync($"GameProccessing:{gameId}", false, false, false);
        await channel.QueueBindAsync($"GameProccessing:{gameId}", ServicesExchanges.GameProccesingExchange, gameId);

        RegisterEventListener(RabbitMqEvents.GetEmptySessionState, true, HandleGetEmptySessionState);
        RegisterEventListener(RabbitMqEvents.ProccessAction, true, HandleProccessAction);
        RegisterEventListener(RabbitMqEvents.GetGameStateForPlayer, true, HandleGetGameStateForPlayer);
        await StartBaseListening(false, $"GameProccessing:{gameId}");
    }

    private async Task<(bool ackSuccess, byte[] replyBody)> HandleGetEmptySessionState(byte[] requestBody)
    {
        try
        {
            GetEmptySessionStateRequest? request = JsonSerializer.Deserialize<GetEmptySessionStateRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await GetEmptySessionState(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the get empty session state message");
            return (false, []);
        }
    }

    private async Task<(bool ackSuccess, byte[] replyBody)> HandleProccessAction(byte[] requestBody)
    {
        try
        {
            ProccessActionRequest? request = JsonSerializer.Deserialize<ProccessActionRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await ProccessAction(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the get proccess action message");
            return (false, []);
        }
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleGetGameStateForPlayer(byte[] requestBody)
    {
        try
        {
            GetGameStateForPlayerRequest? request = JsonSerializer.Deserialize<GetGameStateForPlayerRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await GetGameStateForPlayer(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the get game state for player message");
            return (false, []);
        }
    }

    protected abstract Task<GetEmptySessionStateReply> GetEmptySessionState(GetEmptySessionStateRequest request);
    protected abstract Task<ProccessActionReply> ProccessAction(ProccessActionRequest request);
    protected abstract Task<GetGameStateForPlayerReply> GetGameStateForPlayer(GetGameStateForPlayerRequest request);
}
