namespace SharedApiUtils.RabbitMq.Core.Messages.RoomsService;

public class AddToRoomMessageReply
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}
