using FeatureFlags.Core.Domain;
using FeatureFlags.Infrastructure.Persistence.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Tests.Infrastructure;

public sealed class EntityPersistenceTests
{
  [Fact]
  public async Task CanPersistAndLoad_FeatureFlagEntity()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    await using var _ = db;
    await using var __ = connection;

    var feature = new FeatureFlagEntity
    {
      Id = Guid.NewGuid(),
      Key = "new-search",
      DefaultState = false,
      Description = "desc"
    };

    // Act
    db.FeatureFlags.Add(feature);
    await db.SaveChangesAsync();

    var loaded = await db.FeatureFlags.SingleAsync(x => x.Key == "new-search");

    // Assert
    loaded.DefaultState.Should().BeFalse();
    loaded.Description.Should().Be("desc");
  }

  [Fact]
  public async Task CanPersistAndLoad_FeatureOverrideEntity()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    await using var _ = db;
    await using var __ = connection;

    var featureId = Guid.NewGuid();
    db.FeatureFlags.Add(new FeatureFlagEntity
    {
      Id = featureId,
      Key = "flag-x",
      DefaultState = false
    });

    await db.SaveChangesAsync();

    var ov = new FeatureOverrideEntity
    {
      Id = Guid.NewGuid(),
      FeatureFlagId = featureId,
      Type = OverrideType.User,
      TargetId = "u1",
      State = true
    };

    // Act
    db.FeatureOverrides.Add(ov);
    await db.SaveChangesAsync();

    var loaded = await db.FeatureOverrides.SingleAsync(x => x.Id == ov.Id);

    // Assert
    loaded.FeatureFlagId.Should().Be(featureId);
    loaded.Type.Should().Be(OverrideType.User);
    loaded.TargetId.Should().Be("u1");
    loaded.State.Should().BeTrue();
  }

  [Fact]
  public async Task LoadingFeature_WithOverrides_ShouldIncludeNavigation_WhenExplicitlyIncluded()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    await using var _ = db;
    await using var __ = connection;

    var featureId = Guid.NewGuid();

    db.FeatureFlags.Add(new FeatureFlagEntity
    {
      Id = featureId,
      Key = "flag-with-overrides",
      DefaultState = false
    });

    db.FeatureOverrides.Add(new FeatureOverrideEntity
    {
      Id = Guid.NewGuid(),
      FeatureFlagId = featureId,
      Type = OverrideType.Group,
      TargetId = "beta",
      State = true
    });

    db.FeatureOverrides.Add(new FeatureOverrideEntity
    {
      Id = Guid.NewGuid(),
      FeatureFlagId = featureId,
      Type = OverrideType.Region,
      TargetId = "IN",
      State = false
    });

    await db.SaveChangesAsync();

    // Act
    var loaded = await db.FeatureFlags
        .Include(x => x.Overrides)
        .SingleAsync(x => x.Id == featureId);

    // Assert
    loaded.Overrides.Should().HaveCount(2);
    loaded.Overrides.Select(x => x.Type).Should().Contain(new[] { OverrideType.Group, OverrideType.Region });
  }
}
