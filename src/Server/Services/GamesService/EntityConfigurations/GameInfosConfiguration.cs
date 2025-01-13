using GamesService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GamesService.EntityConfigurations;

public class GameInfosConfiguration : IEntityTypeConfiguration<GameInfoEntity>
{
    public void Configure(EntityTypeBuilder<GameInfoEntity> builder)
    {
        builder.ToTable("GameInfos");
        builder.HasKey(x => x.Id);

        builder.Property(g => g.ImageUrl)
            .IsUnicode(false)
            .HasMaxLength(100);

        builder.Property(g=>g.LocalizationKey)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(g=>g.GameId)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.HasIndex(g => g.GameId).IsUnique();
    }
}
