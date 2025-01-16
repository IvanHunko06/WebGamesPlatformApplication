namespace SharedApiUtils.RabbitMq.Core.Messages.GameProccessingService;

public class GetSessionDeltaMessagesReply
{
    public string? NotifyRoomMessage { get; set; }
    public Dictionary<string, string>? NotifyPlayersMessages { get; set; }
}
