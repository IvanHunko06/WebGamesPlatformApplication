using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RatingService.Entities;

namespace RatingService.EntitiesConfigurations;

public class UserScoreEntityConfiguration : IEntityTypeConfiguration<UserScoreEntity>
{
    public void Configure(EntityTypeBuilder<UserScoreEntity> builder)
    {
        builder.ToTable("UserScores");
        builder.HasKey(x => x.Id);

        builder.HasOne(x=>x.Season)
            .WithMany(x=>x.UserScores)
            .HasForeignKey(x=>x.SeasonId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(x => x.UserId)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(50);
    }
}
