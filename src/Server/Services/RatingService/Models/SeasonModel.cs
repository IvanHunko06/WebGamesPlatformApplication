namespace RatingService.Models;

public class SeasonModel
{
    public int SeasonId { get; set; }
    public DateOnly BeginDate { get; set; }
    public DateOnly EndDate { get; set;}
}
