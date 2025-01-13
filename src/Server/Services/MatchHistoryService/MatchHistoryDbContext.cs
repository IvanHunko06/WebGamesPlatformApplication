using MatchHistoryService.Entities;
using MatchHistoryService.EntitiesConfigurations;
using Microsoft.EntityFrameworkCore;

namespace MatchHistoryService;

public class MatchHistoryDbContext : DbContext
{
    public DbSet<MatchInformationEntity> MatchInformations { get; set; }
    public DbSet<UserScoreEntity> PlayerScores { get; set; }
    public MatchHistoryDbContext(DbContextOptions<MatchHistoryDbContext> options) :
        base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MatchInformationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserScoreEntityConfiguration());
    }
    
}