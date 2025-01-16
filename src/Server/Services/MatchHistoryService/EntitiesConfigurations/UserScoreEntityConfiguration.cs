using MatchHistoryService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatchHistoryService.EntitiesConfigurations;

public class UserScoreEntityConfiguration : IEntityTypeConfiguration<UserScoreEntity>
{
    public void Configure(EntityTypeBuilder<UserScoreEntity> builder)
    {
        builder.ToTable("UserScoreDeltas");

        builder.HasKey(x=>x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(50);

        builder.Property(x => x.ScoreDelta)
            .IsRequired();

        builder
            .HasOne(k => k.MatchInfo)
            .WithMany(x => x.UserScores)
            .HasForeignKey(x => x.MatchInfoId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
