using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Validation;
using FeatureFlags.Infrastructure.Persistence.Entities;
using FeatureFlags.Infrastructure.Stores;
using FluentAssertions;

namespace FeatureFlags.Tests.Infrastructure;

public sealed class FeatureFlagSnapshotLoaderTests
{
  [Fact]
  public async Task LoadAsync_LoadsFeaturesAndOverrides_IntoCache_Normalized()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    await using var _ = db;
    await using var __ = connection;

    var featureId = Guid.NewGuid();

    db.FeatureFlags.Add(new FeatureFlagEntity
    {
      Id = featureId,
      Key = "NewSearch",
      DefaultState = false,
      Description = "desc"
    });

    db.FeatureOverrides.Add(new FeatureOverrideEntity
    {
      Id = Guid.NewGuid(),
      FeatureFlagId = featureId,
      Type = OverrideType.User,
      TargetId = "  U1  ",
      State = true
    });

    db.FeatureOverrides.Add(new FeatureOverrideEntity
    {
      Id = Guid.NewGuid(),
      FeatureFlagId = featureId,
      Type = OverrideType.Region,
      TargetId = "in",
      State = true
    });

    await db.SaveChangesAsync();

    var store = new CachedFeatureFlagStore();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    // Act
    await loader.LoadAsync();

    // Assert
    store.FeatureCount.Should().Be(1);
    store.OverrideCount.Should().Be(2);

    var normalizedKey = FeatureKey.Normalize("NewSearch");
    store.TryGetFeatureByKey(normalizedKey, out var feature).Should().BeTrue();
    feature.Id.Should().Be(featureId);
    feature.Description.Should().Be("desc");

    store.TryGetOverride(featureId, OverrideType.User, OverrideTarget.Normalize("U1"), out var userState).Should().BeTrue();
    userState.Should().BeTrue();

    store.TryGetOverride(featureId, OverrideType.Region, RegionCode.Normalize("IN"), out var regionState).Should().BeTrue();
    regionState.Should().BeTrue();
  }

  [Fact]
  public async Task LoadAsync_ReplacesSnapshot_RemovingOldEntries()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    await using var _ = db;
    await using var __ = connection;

    var store = new CachedFeatureFlagStore();

    // seed store with something old
    var old = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("old-flag"), defaultState: true, description: null);
    store.ReplaceSnapshot(
      new Dictionary<string, FeatureFlag> { [old.Key] = old },
      new Dictionary<(Guid, OverrideType, string), bool>()
    );

    var newId = Guid.NewGuid();
    db.FeatureFlags.Add(new FeatureFlagEntity { Id = newId, Key = "new-flag", DefaultState = false });
    await db.SaveChangesAsync();

    var loader = new FeatureFlagSnapshotLoader(db, store);

    // Act
    await loader.LoadAsync();

    // Assert
    store.FeatureCount.Should().Be(1);
    store.TryGetFeatureByKey(FeatureKey.Normalize("old-flag"), out var _).Should().BeFalse();
    store.TryGetFeatureByKey(FeatureKey.Normalize("new-flag"), out var _).Should().BeTrue();
  }
}
