using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedApiUtils.Abstractons;
using System.Text;
using System.Text.Json;

namespace SharedApiUtils.RabbitMq.Clients;

public class RabbitMqBaseClient
{
    private readonly RabbitMqConnection connection;
    private readonly ILogger<RabbitMqBaseClient> logger;

    public RabbitMqBaseClient(RabbitMqConnection connection, ILogger<RabbitMqBaseClient> logger)
    {
        this.connection = connection;
        this.logger = logger;
    }
    public async Task<Resp> SendRequest<Req, Resp>(Req requestBody, string eventName, string routingKey, string exchange = "")
    where Resp : class, new()
    where Req : class
    {
        string correlationId = Guid.NewGuid().ToString();
        var responseTaskSource = new TaskCompletionSource<Resp>();
        IChannel? channel = null;
        QueueDeclareOk? responseQueue = null;
        AsyncEventingBasicConsumer? consumer = null;
        async Task OnRecived(object sender, BasicDeliverEventArgs ea)
        {
            logger.LogDebug($"Message recived to queue: {responseQueue?.QueueName}. CorrelationId: {correlationId}");
            if (ea.BasicProperties.CorrelationId != correlationId) return;
            try
            {
                var responseBody = Encoding.UTF8.GetString(ea.Body.ToArray());
                logger.LogDebug($"Response recived. CorrelationId: {correlationId}, EventType: {eventName}, ResponseBody: {responseBody}");
                var reply = JsonSerializer.Deserialize<Resp>(responseBody);
                responseTaskSource.TrySetResult(reply ?? new Resp());
            }
            catch (Exception ex)
            {
                responseTaskSource.TrySetException(ex);
            }
        }
        try
        {
            channel = await connection.GetNewChannel();
            responseQueue = await channel.QueueDeclareAsync(exclusive: true, autoDelete: true);
            consumer = new AsyncEventingBasicConsumer(channel);


            consumer.ReceivedAsync += OnRecived;

            await channel.BasicConsumeAsync(responseQueue.QueueName, true, consumer);

            var properties = new BasicProperties
            {
                CorrelationId = correlationId,
                ReplyTo = responseQueue.QueueName,
                Headers = new Dictionary<string, object?>
                {
                    [WellKnownHeaders.EventType] = eventName,
                    [WellKnownHeaders.BodyType] = nameof(Req)
                },
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(requestBody));
            logger.LogDebug($"Sending request message via channel {channel.ChannelNumber}. Routing key: {routingKey}, Exchange: {exchange}, " +
                $"CorrelationId: {correlationId}, EventType: {eventName}, " +
                $"Response queue: {responseQueue.QueueName}. RequestBody: {JsonSerializer.Serialize(requestBody)}");
            await channel.BasicPublishAsync(exchange: exchange,
                                                routingKey: routingKey,
                                                basicProperties: properties,
                                                body: body,
                                                mandatory: false);
            var replyTask = responseTaskSource.Task;
            var waitTask = Task.Delay(TimeSpan.FromSeconds(60));
            var firstCompletedTask = await Task.WhenAny(replyTask, waitTask);
            if (firstCompletedTask == waitTask)
                throw new TimeoutException(ErrorMessages.TimeoutExceeded);
            return replyTask.Result;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            if (channel is not null && responseQueue is not null)
            {
                await channel.QueueDeleteAsync(responseQueue.QueueName);
                logger.LogDebug($"Queue deleted: {responseQueue.QueueName}");
                
            }
            if(consumer is not null)
                consumer.ReceivedAsync -= OnRecived;

            channel?.Dispose();
        }
    }

}
