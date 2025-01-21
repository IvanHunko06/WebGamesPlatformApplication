using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfileService.Entities;

namespace ProfileService.EntitiesConfigurations;

public class ProfileImageEntityConfiguration : IEntityTypeConfiguration<ProfileImageEntity>
{
    public void Configure(EntityTypeBuilder<ProfileImageEntity> builder)
    {
        builder.HasIndex(x=>x.Id);

        builder.ToTable("ProfileImages");

        builder
            .Property(x => x.SmallImageUrl)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(100);

        builder
            .Property(x => x.BigImageUrl)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(100);

        builder
            .HasMany(x => x.Profiles)
            .WithOne(x => x.ProfileImage)
            .HasForeignKey(x => x.ImageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
