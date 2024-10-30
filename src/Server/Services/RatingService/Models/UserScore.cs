namespace RatingService.Models
{
    public class UserScore
    {
        public int UserScoreId { get; set; }
        public string UserId { get; set; }
        public int Score { get; set; }
        public int SeasonId { get; set; }

        public Season Season { get; set; }
    }

}
