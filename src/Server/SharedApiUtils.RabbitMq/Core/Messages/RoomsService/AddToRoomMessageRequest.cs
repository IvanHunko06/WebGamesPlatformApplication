namespace SharedApiUtils.RabbitMq.Core.Messages.RoomsService;

public class AddToRoomMessageRequest
{
    public string UserId { get; set; }
    public string RoomId { get; set; }
    public string AccessToken { get; set; }
}
