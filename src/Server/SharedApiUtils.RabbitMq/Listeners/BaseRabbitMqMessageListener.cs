using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace SharedApiUtils.RabbitMq.Listeners;

public abstract class BaseRabbitMqMessageListener :IDisposable
{
    private readonly RabbitMqConnection connection;
    private readonly ILogger<BaseRabbitMqMessageListener> logger;
    private readonly Dictionary<string, EventProccessor> registeredEvents;
    private readonly Dictionary<string, bool> eventNeedReply;
    private IChannel? channel;
    private bool autoAck;

    public delegate Task<(bool ackSuccess, byte[] replyBody)> EventProccessor(byte[] requestBody);
    public BaseRabbitMqMessageListener(RabbitMqConnection connection, ILogger<BaseRabbitMqMessageListener> logger)
    {
        this.connection = connection;
        this.logger = logger;
        registeredEvents = new Dictionary<string, EventProccessor>();
        eventNeedReply = new Dictionary<string, bool>();
    }
    public void Dispose()
    {
        channel?.Dispose();
    }
    internal async Task StartBaseListening(bool autoAck, string queueName, IChannel channel)
    {
        this.channel = channel;
        logger.LogDebug($"Using channel {channel.ChannelNumber} for queue {queueName}");
        var consumer = new AsyncEventingBasicConsumer(channel);
        this.autoAck = autoAck;
        consumer.ReceivedAsync += Consumer_ReceivedAsync;
        await channel.BasicConsumeAsync(
            queueName,
            autoAck: autoAck,
            consumer: consumer,
            consumerTag: Guid.NewGuid().ToString(),
            exclusive: true,
            noLocal: true,
            arguments: null);
    }

    private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        if (channel is null) return;
        var body = ea.Body.ToArray();
        var props = ea.BasicProperties;
        if (props.Headers is null)
        {
            if (!autoAck)
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            return;
        }
        if (!props.Headers.TryGetValue(WellKnownHeaders.EventType, out object? requestedActionObject) || requestedActionObject is not byte[])
        {
            logger.LogWarning("RabbitMq message does not contain header EventType.");
            if (!autoAck)
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            return;
        }
        string requestedAction = Encoding.UTF8.GetString((byte[])requestedActionObject);
        if (!registeredEvents.ContainsKey(requestedAction) || !eventNeedReply.ContainsKey(requestedAction))
        {
            logger.LogWarning($"RabbitMq event {requestedAction} not registered");
            if (!autoAck)
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            return;
        }
        bool needReply = eventNeedReply[requestedAction];
        if (needReply && (string.IsNullOrEmpty(props.ReplyTo) || string.IsNullOrEmpty(props.CorrelationId)))
        {
            logger.LogWarning($"ReplyTo or CorrelationId is null");
            if (!autoAck)
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            return;
        }
        logger.LogDebug($"Request reviced. ReplyTo: {props.ReplyTo ?? ""}, CorrelationId: {props.CorrelationId ?? ""}, ActionName: {requestedAction}");
        bool hasException = false;
        bool alreadyConfirmed = false;
        try
        {
            var actionResult = await registeredEvents[requestedAction](body);

            if (needReply && actionResult.ackSuccess)
            {
                var replyProps = new BasicProperties();
                replyProps.CorrelationId = props.CorrelationId;
                logger.LogDebug($"Sending response to {props.ReplyTo}. CorrelationId: {props.CorrelationId}. Response body: {Encoding.UTF8.GetString(actionResult.replyBody)}");
                await channel.BasicPublishAsync("", props.ReplyTo, false, replyProps, actionResult.replyBody);
            }
            if (!autoAck)
            {
                if (actionResult.ackSuccess)
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                else
                    await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                alreadyConfirmed = true;
            }

        }
        catch (Exception ex)
        {
            hasException = true;
            logger.LogError(ex, "An error occurred in base message listening");
        }
        finally
        {
            if (!autoAck && !alreadyConfirmed)
            {
                if (!hasException)
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                else
                    await channel.BasicNackAsync(ea.DeliveryTag, false, false);
            }
        }
    }

    internal void RegisterEventListener(string eventName, bool needReply, EventProccessor eventProccessor)
    {
        registeredEvents[eventName] = eventProccessor;
        eventNeedReply[eventName] = needReply;
    }
    internal IChannel? GetChannel()
    {
        return channel;
    }
}
