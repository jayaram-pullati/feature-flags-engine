using FeatureFlags.Infrastructure.Persistence.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Tests.Infrastructure;

public sealed class FeatureFlagsDbContextTests
{
  [Fact]
  public void Model_ShouldContain_FeatureFlagEntity_And_FeatureOverrideEntity()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    using var _ = db;
    using var __ = connection;

    // Act
    var featureEntity = db.Model.FindEntityType(typeof(FeatureFlagEntity));
    var overrideEntity = db.Model.FindEntityType(typeof(FeatureOverrideEntity));

    // Assert
    featureEntity.Should().NotBeNull();
    overrideEntity.Should().NotBeNull();
  }

  [Fact]
  public async Task DbSets_ShouldBeQueryable()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    await using var _ = db;
    using var __ = connection;

    // Act
    var featureCount = await db.FeatureFlags.CountAsync();
    var overrideCount = await db.FeatureOverrides.CountAsync();

    // Assert
    featureCount.Should().Be(0);
    overrideCount.Should().Be(0);
  }
}
