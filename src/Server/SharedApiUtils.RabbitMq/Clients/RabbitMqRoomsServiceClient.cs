using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Core;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.RabbitMq.Core.Messages.RoomsService;
using System.Text;
using System.Text.Json;

namespace SharedApiUtils.RabbitMq.Clients;

public class RabbitMqRoomsServiceClient : RabbitMqBaseClient, IRoomsServiceClient
{
    private readonly ILogger<RabbitMqRoomsServiceClient> logger;

    public RabbitMqRoomsServiceClient(
        RabbitMqConnection connection, 
        ILogger<RabbitMqRoomsServiceClient> logger, 
        RabbitMqMessagePublisher messagePublisher) : base(connection, messagePublisher)
    {
        this.logger = logger;
    }

    public async Task<string?> AddToRoom(string roomId, string userId, string accessToken)
    {
        try
        {
            var request = new AddToRoomMessageRequest { RoomId = roomId, UserId = userId, AccessToken = accessToken };
            var reply = await SendRequest<AddToRoomMessageRequest, AddToRoomMessageReply>(request, RabbitMqEvents.AddUserToRoom, ServicesQueues.RoomsServiceQueue);
            return reply.IsSuccess ? null : reply.ErrorMessage;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to add a user to the room.");
            return ErrorMessages.InternalServerError;
        }
    }

    public async Task<string?> DeleteRoom(string roomId)
    {
        try
        {
            var request = new DeleteRoomRequest { RoomId = roomId};
            var reply = await SendRequest<DeleteRoomRequest, DeleteRoomReply>(request, RabbitMqEvents.DeleteRoom, ServicesQueues.RoomsServiceQueue);
            return reply.IsSuccess ? null : reply.ErrorMessage;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to delete the room.");
            return ErrorMessages.InternalServerError;
        }
    }

    public async Task<(RoomModelDto?, string?)> GetRoom(string roomId)
    {
        try
        {
            var request = new GetRoomRequest { RoomId = roomId };
            var reply = await SendRequest<GetRoomRequest, GetRoomReply>(request, RabbitMqEvents.GetRoom, ServicesQueues.RoomsServiceQueue);
            return reply.IsSuccess ? (reply.Room, null):(null, reply.ErrorMessage); 
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to get the room.");
            return (null, ErrorMessages.InternalServerError);
        }
    }

    public async Task<string?> RemoveFromRoom(string roomId, string userId)
    {
        try
        {
            var request = new RemoveFromRoomMessageRequest { RoomId = roomId, UserId = userId };
            var reply = await SendRequest<RemoveFromRoomMessageRequest, RemoveFromRoomMessageReply>(request, RabbitMqEvents.RemoveUserFromRoom, ServicesQueues.RoomsServiceQueue);
            return reply.IsSuccess ? null : reply.ErrorMessage;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending a message to remove user from the room.");
            return ErrorMessages.InternalServerError;
        }
    }
}

