namespace SharedApiUtils.RabbitMq.Core.Messages.GameProccessingService;

public class ProccessActionRequest
{
    public string SessionState { get; set; }
    public string Action { get; set; }
    public string Payload { get; set; }
    public string UserId { get; set; }

}
