using FeatureFlags.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeatureFlags.Infrastructure.Persistence.Configurations;

public sealed class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlagEntity>
{
  public void Configure(EntityTypeBuilder<FeatureFlagEntity> builder)
  {
    builder.ToTable("FeatureFlags");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Key)
        .HasMaxLength(100)
        .IsRequired();

    builder.HasIndex(x => x.Key)
        .IsUnique();

    builder.Property(x => x.Description)
        .HasMaxLength(1000);

    builder.HasMany(x => x.Overrides)
        .WithOne(x => x.FeatureFlag)
        .HasForeignKey(x => x.FeatureFlagId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
