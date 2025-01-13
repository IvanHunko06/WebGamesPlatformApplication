using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SharedApiUtils.Abstractons.Core;
using SharedApiUtils.Abstractons.Interfaces.Clients;

namespace SharedApiUtils.RabbitMq.Clients;

public class RabbitMQGamesServiceClient : IGamesServiceClient
{
    private readonly IChannel channel;
    private readonly ILogger<RabbitMQGamesServiceClient> logger;

    public RabbitMQGamesServiceClient(RabbitMqConnection rabbitMq, ILogger<RabbitMQGamesServiceClient> logger)
    {
        channel = rabbitMq.GetChannel();
        this.logger = logger;
    }

    public Task<GameInfoDto?> GetGameInfo(string gameId)
    {
        throw new NotImplementedException();
    }
    //public async Task<GameInfo?> GetGameInfo(string gameId)
    //{
    //    try
    //    {
    //        string replyQueueName = (await channel.QueueDeclareAsync(queue: ServicesQueues.RoomsServiceQueue, autoDelete: false, durable: true)).QueueName;
    //        string correlationId = Guid.NewGuid().ToString();
    //        BasicProperties props = new BasicProperties()
    //        {
    //            CorrelationId = correlationId,
    //            ReplyTo = replyQueueName,
    //            Timestamp = new AmqpTimestamp()
    //        };
    //        RabbitMqServiceRequestMessageBasic request = new RabbitMqServiceRequestMessageBasic()
    //        {
    //            RequestMethod = "GetGameInfo",
    //            Payload = new RabbitMqGetGameInfoRequest()
    //            {
    //                GameId = gameId
    //            }
    //        };
    //        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));
    //        await channel.BasicPublishAsync(exchange: "", routingKey: ServicesQueues.GamesServiceQueue, basicProperties: props, body: body, mandatory: false);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "");
    //    }

    //    return null;

    //}
}
