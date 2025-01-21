using Microsoft.EntityFrameworkCore;
using ProfileService.Entities;
using ProfileService.EntitiesConfigurations;
namespace ProfileService;

public class ProfileServiceDbContext : DbContext
{
    public DbSet<ProfileEntity> Profiles { get; set; }
    public DbSet<ProfileImageEntity> ProfileImages { get; set; }
    public ProfileServiceDbContext(DbContextOptions<ProfileServiceDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProfileEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProfileImageEntityConfiguration());
    }
}
