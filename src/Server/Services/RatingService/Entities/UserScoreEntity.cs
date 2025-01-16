namespace RatingService.Entities;

public class UserScoreEntity
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int Score { get; set; }
    public int SeasonId { get; set; }
    public SeasonEntity Season { get; set; }
}
