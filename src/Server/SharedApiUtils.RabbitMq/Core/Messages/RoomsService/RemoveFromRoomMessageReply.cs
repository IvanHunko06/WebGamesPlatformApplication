namespace SharedApiUtils.RabbitMq.Core.Messages.RoomsService;

public class RemoveFromRoomMessageReply
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}
