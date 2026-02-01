using FeatureFlags.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Infrastructure.Persistence;

public sealed class FeatureFlagsDbContext(DbContextOptions<FeatureFlagsDbContext> options)
    : DbContext(options)
{
  public DbSet<FeatureFlagEntity> FeatureFlags => Set<FeatureFlagEntity>();
  public DbSet<FeatureOverrideEntity> FeatureOverrides => Set<FeatureOverrideEntity>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(FeatureFlagsDbContext).Assembly);
  }
}
