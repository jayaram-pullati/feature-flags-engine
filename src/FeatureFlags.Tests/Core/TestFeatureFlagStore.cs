using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Evaluation;
using FeatureFlags.Core.Validation;

namespace FeatureFlags.Tests.Core;

public sealed class TestFeatureFlagStore : IFeatureFlagStore
{
  private readonly Dictionary<string, FeatureFlag> _features = new(StringComparer.OrdinalIgnoreCase);

  private readonly Dictionary<(Guid FeatureFlagId, OverrideType Type, string TargetId), bool> _overrides = new();

  public TestFeatureFlagStore AddFeature(string key, bool defaultState, string? description = "test")
  {
    var normalizedKey = FeatureKey.Normalize(key);
    var feature = new FeatureFlag(Guid.NewGuid(), normalizedKey, defaultState, description);
    _features[normalizedKey] = feature;
    return this;
  }

  public TestFeatureFlagStore AddFeature(FeatureFlag feature)
  {
    _features[feature.Key] = feature;
    return this;
  }

  public TestFeatureFlagStore AddOverride(string featureKey, OverrideType type, string normalizedTargetId, bool state)
  {
    var normalizedKey = FeatureKey.Normalize(featureKey);

    if (!_features.TryGetValue(normalizedKey, out var feature))
      throw new InvalidOperationException($"Feature '{normalizedKey}' not found. AddFeature first.");

    _overrides[(feature.Id, type, normalizedTargetId)] = state;
    return this;
  }

  public bool TryGetFeatureByKey(string normalizedKey, out FeatureFlag feature)
    => _features.TryGetValue(normalizedKey, out feature!);

  public bool TryGetOverride(Guid featureFlagId, OverrideType type, string normalizedTargetId, out bool state)
    => _overrides.TryGetValue((featureFlagId, type, normalizedTargetId), out state);
}
