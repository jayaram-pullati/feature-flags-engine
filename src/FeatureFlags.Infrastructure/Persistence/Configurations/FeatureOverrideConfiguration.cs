using FeatureFlags.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeatureFlags.Infrastructure.Persistence.Configurations;

public sealed class FeatureOverrideConfiguration : IEntityTypeConfiguration<FeatureOverrideEntity>
{
  public void Configure(EntityTypeBuilder<FeatureOverrideEntity> builder)
  {
    builder.ToTable("FeatureOverrides");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.TargetId)
        .HasMaxLength(200)
        .IsRequired();

    builder.HasIndex(x => new { x.FeatureFlagId, x.Type, x.TargetId })
        .IsUnique();

    builder.Property(x => x.Type)
        .IsRequired();
  }
}
