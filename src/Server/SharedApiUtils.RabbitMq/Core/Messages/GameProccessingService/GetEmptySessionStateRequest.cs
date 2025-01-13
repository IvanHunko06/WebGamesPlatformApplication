namespace SharedApiUtils.RabbitMq.Core.Messages.GameProccessingService;

public class GetEmptySessionStateRequest
{
    public IEnumerable<string> Players { get; set; }
}
