using Microsoft.EntityFrameworkCore;
using RatingService.Models;

namespace RatingService
{
    public class RatingDbContext : DbContext
    {
        public RatingDbContext(DbContextOptions<RatingDbContext> options) :
            base(options)
        {
        }

        public DbSet<Season> Seasons { get; set; }
        public DbSet<UserScore> UserScores { get; set; }
    }

}
