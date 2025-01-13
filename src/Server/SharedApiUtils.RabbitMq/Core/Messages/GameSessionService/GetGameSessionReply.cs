using SharedApiUtils.Abstractons.Core;

namespace SharedApiUtils.RabbitMq.Core.Messages.GameSessionService;

public class GetGameSessionReply
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public GameSessionDto? GameSession { get; set; }
}
