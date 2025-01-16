namespace SharedApiUtils.RabbitMq.Core.Messages.RoomsService;

public class RemoveFromRoomMessageRequest
{
    public string UserId { get; set; }
    public string RoomId { get; set; }
}
