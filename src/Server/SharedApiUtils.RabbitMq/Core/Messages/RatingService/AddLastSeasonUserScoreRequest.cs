namespace SharedApiUtils.RabbitMq.Core.Messages.RatingService;

public class AddLastSeasonUserScoreRequest
{
    public string UserId { get; set; }
    public int AddScore { get; set; }
}
