using Microsoft.Extensions.Logging;
using SharedApiUtils.RabbitMq.Core.Messages.RoomsService;
using System.Text.Json;

namespace SharedApiUtils.RabbitMq.Listeners;

public abstract class BaseRabbitMqRoomsServiceListener : BaseRabbitMqMessageListener
{
    private readonly RabbitMqConnection connection;
    private readonly ILogger<BaseRabbitMqRoomsServiceListener> logger;

    public BaseRabbitMqRoomsServiceListener(RabbitMqConnection connection,
        ILogger<BaseRabbitMqRoomsServiceListener> logger,
        ILogger<BaseRabbitMqMessageListener> _logger1) : base(connection, _logger1)
    {
        this.connection = connection;
        this.logger = logger;
    }
    public async Task StartListening()
    {
        var channel = connection.GetChannel();
        var queue = await channel.QueueDeclareAsync(
            queue: ServicesQueues.RoomsServiceQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        RegisterEventListener(RabbitMqEvents.AddUserToRoom, true, HandleAddToRoom);
        RegisterEventListener(RabbitMqEvents.RemoveUserFromRoom, true, HandleRemoveFromRoom);
        RegisterEventListener(RabbitMqEvents.DeleteRoom, true, HandleDeleteRoom);
        RegisterEventListener(RabbitMqEvents.GetRoom, true, HandleGetRoom);
        await StartBaseListening(false, ServicesQueues.RoomsServiceQueue);
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleAddToRoom(byte[] requestBody)
    {
        try
        {
            AddToRoomMessageRequest? request = JsonSerializer.Deserialize<AddToRoomMessageRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await AddToRoom(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the add to room message");
            return (false, []);
        }
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleRemoveFromRoom(byte[] requestBody)
    {
        try
        {
            RemoveFromRoomMessageRequest? request = JsonSerializer.Deserialize<RemoveFromRoomMessageRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await RemoveFromRoom(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the remove from room message.");
            return (false, []);
        }
    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleDeleteRoom(byte[] requestBody)
    {
        try
        {
            DeleteRoomRequest? request = JsonSerializer.Deserialize<DeleteRoomRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await DeleteRoom(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the delete room message");
            return (false, []);
        }

    }
    private async Task<(bool ackSuccess, byte[] replyBody)> HandleGetRoom(byte[] requestBody)
    {
        try
        {
            GetRoomRequest? request = JsonSerializer.Deserialize<GetRoomRequest>(requestBody);
            if (request is null)
                return (false, []);
            var reply = await GetRoom(request);
            var replyBytes = JsonSerializer.SerializeToUtf8Bytes(reply);
            return (true, replyBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while receiving the get room message");
            return (false, []);
        }
    }

    protected abstract Task<AddToRoomMessageReply> AddToRoom(AddToRoomMessageRequest request);
    protected abstract Task<RemoveFromRoomMessageReply> RemoveFromRoom(RemoveFromRoomMessageRequest request);
    protected abstract Task<DeleteRoomReply> DeleteRoom(DeleteRoomRequest request);
    protected abstract Task<GetRoomReply> GetRoom(GetRoomRequest request);
}
