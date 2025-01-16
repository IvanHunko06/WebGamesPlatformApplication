using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RatingService.Entities;

namespace RatingService.EntitiesConfigurations;

public class SeasonEntityConfiguration : IEntityTypeConfiguration<SeasonEntity>
{
    public void Configure(EntityTypeBuilder<SeasonEntity> builder)
    {
        builder.ToTable("Seasons");

        builder.HasKey(x => x.Id);

        builder
            .HasMany(x => x.UserScores)
            .WithOne(x => x.Season)
            .HasForeignKey(x => x.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(x => x.DateStart)
            .IsUnique();
        
        builder
            .HasIndex(x=>x.DateEnd)
            .IsUnique();

        builder
            .Property(x => x.DateStart)
            .HasColumnType("date");

        builder
            .Property(x => x.DateEnd)
            .HasColumnType("date");
    }
}
