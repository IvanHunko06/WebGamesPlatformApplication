using SharedApiUtils.Abstractons.Core;

namespace SharedApiUtils.RabbitMq.Core.Messages.RoomsService;

public class GetRoomReply
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public RoomModelDto? Room { get; set; }
}
