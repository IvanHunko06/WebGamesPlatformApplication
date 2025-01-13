using RoomsService.Interfaces;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Core;
using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Core.Messages.RoomsService;
using SharedApiUtils.RabbitMq.Listeners;

namespace RoomsService.Services;

public sealed class RoomsRabbitMqService : BaseRabbitMqRoomsServiceListener
{
    private readonly ILogger<RoomsRabbitMqService> logger;
    private readonly IRoomsService roomsService;

    public RoomsRabbitMqService(ILogger<RoomsRabbitMqService> logger, 
        RabbitMqConnection connection, 
        IRoomsService roomsService, 
        ILogger<BaseRabbitMqRoomsServiceListener> _logger1, 
        ILogger<BaseRabbitMqMessageListener> _logger2,  
        RabbitMqMessagePublisher _messagePublisher) :base(connection, _logger1, _logger2, _messagePublisher)
    {
        this.logger = logger;
        this.roomsService = roomsService;
    }
    protected override async Task<AddToRoomMessageReply> AddToRoom(AddToRoomMessageRequest request)
    {
        logger.LogInformation($"RabbitMq event AddToRoom called");
        AddToRoomMessageReply reply = new AddToRoomMessageReply();

        string? errorMessage = await roomsService.AddUserToRoom(request.RoomId, request.UserId, request.AccessToken);
        if(string.IsNullOrEmpty(errorMessage))
            reply.IsSuccess = true;
        else
            reply.ErrorMessage = errorMessage;

        return reply;
    }

    protected override async Task<DeleteRoomReply> DeleteRoom(DeleteRoomRequest request)
    {
        logger.LogInformation($"RabbitMq event DeleteRoom called");
        DeleteRoomReply reply = new DeleteRoomReply();

        string? errorMessage = await roomsService.DeleteRoom(request.RoomId);
        if(string.IsNullOrEmpty (errorMessage))
            reply.IsSuccess = true;
        else
            reply.ErrorMessage = errorMessage;

        return reply;

    }

    protected override async Task<GetRoomReply> GetRoom(GetRoomRequest request)
    {
        logger.LogInformation($"RabbitMq event GetRoom called");
        GetRoomReply reply = new GetRoomReply();

        var room = await roomsService.GetRoom(request.RoomId);
        if (room is null)
        {
            reply.ErrorMessage = ErrorMessages.RoomIdNotExist;
            return reply;
        }
        reply.IsSuccess = true;
        reply.Room = new RoomModelDto()
        {
            Creator = room.Creator,
            SelectedPlayerCount = room.SelectedPlayersCount,
            GameId = room.GameId,
            IsPrivate = room.IsPrivate,
            Members = room.Members,
            RoomId = room.RoomId,
            RoomName = room.RoomName,
        };
        return reply;
            

    }

    protected override async Task<RemoveFromRoomMessageReply> RemoveFromRoom(RemoveFromRoomMessageRequest request)
    {
        logger.LogInformation($"RabbitMq event RemoveFromRoom called");
        RemoveFromRoomMessageReply reply = new RemoveFromRoomMessageReply();
        string? errorMessage = await roomsService.RemoveUserFromRoom(request.UserId, request.RoomId);
        if (string.IsNullOrEmpty(errorMessage))
            reply.IsSuccess = true;
        else
            reply.ErrorMessage = errorMessage;

        return reply;
    }
}
