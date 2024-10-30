using GamesService.EntityConfigurations;
using GamesService.Models;
using Microsoft.EntityFrameworkCore;

namespace GamesService;

public class GamesServerDbContext :DbContext
{
    public GamesServerDbContext(DbContextOptions<GamesServerDbContext> options) : base(options)
    {
           
    }

    public DbSet<GameInfoEntity> GameInfos { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new GameInfosConfiguration());
    }
}
