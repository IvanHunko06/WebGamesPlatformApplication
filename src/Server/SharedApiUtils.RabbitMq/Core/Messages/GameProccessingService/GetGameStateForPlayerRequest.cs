namespace SharedApiUtils.RabbitMq.Core.Messages.GameProccessingService;

public class GetGameStateForPlayerRequest
{
    public string GameState {  get; set; }
    public string UserId {  get; set; }
}
