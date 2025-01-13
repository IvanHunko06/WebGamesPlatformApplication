namespace SharedApiUtils.RabbitMq.Core.Messages.GameSessionService;

public class SyncGameStateRequest
{
    public string PlayerId {  get; set; }
    public string SessionId { get; set; }
}
