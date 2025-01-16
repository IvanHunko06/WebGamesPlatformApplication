using Microsoft.AspNetCore.Identity;

namespace RatingService.Entities;

public class SeasonEntity
{
    public int Id { get; set; }
    public DateOnly DateStart { get; set; }
    public DateOnly DateEnd { get; set; }

    public ICollection<UserScoreEntity> UserScores { get; set; }
}
