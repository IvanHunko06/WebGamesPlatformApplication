using MatchHistoryService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatchHistoryService.EntitiesConfigurations;

public class MatchInformationEntityConfiguration : IEntityTypeConfiguration<MatchInformationEntity>
{
    public void Configure(EntityTypeBuilder<MatchInformationEntity> builder)
    {
        builder.ToTable("MatchInformations");

        builder.HasKey(x=>x.Id);
        
        builder
            .Property(x => x.FinishReason)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(20);

        builder
            .HasMany(x => x.UserScores)
            .WithOne(x => x.MatchInfo)
            .HasForeignKey(x => x.MatchInfoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(x => x.TimeBegin)
            .IsRequired();

        builder
            .Property(x=>x.TimeEnd)
            .IsRequired();

        builder
            .Property(x => x.GameId)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(30);

        builder
            .HasIndex(x => x.RecordId)
            .IsUnique();

        builder
            .Property(x => x.RecordId)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(50);
    }
}
