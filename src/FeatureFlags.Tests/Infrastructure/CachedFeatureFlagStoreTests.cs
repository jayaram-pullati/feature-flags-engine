using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Validation;
using FeatureFlags.Infrastructure.Stores;
using FluentAssertions;

namespace FeatureFlags.Tests.Infrastructure;

public sealed class CachedFeatureFlagStoreTests
{
  [Fact]
  public void ReplaceSnapshot_ReplacesAllDataAtomically()
  {
    // Arrange
    var store = new CachedFeatureFlagStore();

    var f1 = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("flag-a"), defaultState: false, description: null);
    var f2 = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("flag-b"), defaultState: true, description: null);

    var features = new Dictionary<string, FeatureFlag>(StringComparer.OrdinalIgnoreCase)
    {
      [f1.Key] = f1,
      [f2.Key] = f2
    };

    var overrides = new Dictionary<(Guid, OverrideType, string), bool>
    {
      [(f1.Id, OverrideType.User, OverrideTarget.Normalize("u1"))] = true
    };

    // Act
    store.ReplaceSnapshot(features, overrides);

    // Assert
    store.FeatureCount.Should().Be(2);
    store.OverrideCount.Should().Be(1);

    store.TryGetFeatureByKey(f1.Key, out var loaded).Should().BeTrue();
    loaded.Id.Should().Be(f1.Id);

    store.TryGetOverride(f1.Id, OverrideType.User, OverrideTarget.Normalize("u1"), out var state).Should().BeTrue();
    state.Should().BeTrue();
  }
}
