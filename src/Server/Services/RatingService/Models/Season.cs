using Microsoft.AspNetCore.Identity;

namespace RatingService.Models
{
    public class Season
    {
        public int SeasonId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }

        public ICollection<UserScore> UserScores { get; set; }
    }
}
