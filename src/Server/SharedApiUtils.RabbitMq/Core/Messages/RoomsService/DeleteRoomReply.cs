namespace SharedApiUtils.RabbitMq.Core.Messages.RoomsService;

public class DeleteRoomReply
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
}
