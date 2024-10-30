using MatchHistoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace MatchHistoryService;

public class MatchHistoryDbContext : DbContext
{
    public MatchHistoryDbContext(DbContextOptions<MatchHistoryDbContext> options) :
        base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
    public DbSet<MatchInformation> MatchInformations { get; set; }
    public DbSet<Score> PlayerScores { get; set; }
}