using Microsoft.EntityFrameworkCore;
using RatingService.Entities;
using RatingService.EntitiesConfigurations;

namespace RatingService;

public class RatingDbContext : DbContext
{
    public RatingDbContext(DbContextOptions<RatingDbContext> options) :
        base(options)
    {
    }

    public DbSet<SeasonEntity> Seasons { get; set; }
    public DbSet<UserScoreEntity> UserScores { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SeasonEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserScoreEntityConfiguration());
    }
}
