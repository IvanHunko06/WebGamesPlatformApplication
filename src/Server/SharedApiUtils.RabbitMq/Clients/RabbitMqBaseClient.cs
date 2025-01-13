using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedApiUtils.Abstractons;
using System.Text;
using System.Text.Json;

namespace SharedApiUtils.RabbitMq.Clients;

public class RabbitMqBaseClient
{
    private readonly RabbitMqConnection connection;
    private readonly RabbitMqMessagePublisher messagePublisher;

    public RabbitMqBaseClient(RabbitMqConnection connection, RabbitMqMessagePublisher messagePublisher)
    {
        this.connection = connection;
        this.messagePublisher = messagePublisher;
    }
    public async Task<Resp> SendRequest<Req, Resp>(Req requestBody, string eventName, string routingKey, string exchange = "")
    where Resp : class, new()
    where Req : class
    {
        string correlationId = Guid.NewGuid().ToString();
        var responseTaskSource = new TaskCompletionSource<Resp>();
        IChannel? channel = null;
        QueueDeclareOk? responseQueue = null;
        var consumer = new AsyncEventingBasicConsumer(channel);
        async Task OnRecived(object sender, BasicDeliverEventArgs ea)
        {
            if (ea.BasicProperties.CorrelationId != correlationId) return;
            try
            {
                var responseBody = Encoding.UTF8.GetString(ea.Body.ToArray());
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
            channel = connection.GetChannel();
            responseQueue = await channel.QueueDeclareAsync(exclusive: true, autoDelete: true);

            
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
            //await messagePublisher.PublishWithRetry(channel, exchange, routingKey, properties, body, 1);
            await channel.BasicPublishAsync(exchange: exchange,
                                                routingKey: routingKey,
                                                basicProperties: properties,
                                                body: body,
                                                mandatory: false);
            var replyTask = responseTaskSource.Task;
            var waitTask = Task.Delay(TimeSpan.FromSeconds(30));
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
            }
            consumer.ReceivedAsync -= OnRecived;
        }
    }

}
