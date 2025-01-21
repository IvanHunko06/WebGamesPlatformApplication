using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfileService.Entities;

namespace ProfileService.EntitiesConfigurations;

public class ProfileEntityConfiguration : IEntityTypeConfiguration<ProfileEntity>
{
    public void Configure(EntityTypeBuilder<ProfileEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.ToTable("Profiles");

        builder.Property(x => x.Username)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(50);

        builder
            .HasIndex(x => x.Username)
            .IsUnique();

        builder.Property(x=>x.PublicName)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(50);

        builder
            .Property(x => x.DOB)
            .HasColumnType("date");

        builder
            .Property(x => x.IsPrivateProfile)
            .IsRequired()
            .HasDefaultValue(false);

        builder
            .HasOne(x => x.ProfileImage)
            .WithMany(x => x.Profiles)
            .HasForeignKey(x=>x.ImageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
