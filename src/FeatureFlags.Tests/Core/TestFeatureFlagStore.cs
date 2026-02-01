using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Evaluation;
using FeatureFlags.Core.Validation;

namespace FeatureFlags.Tests.Core;

internal sealed class TestFeatureFlagStore : IFeatureFlagStore
{
  private readonly Dictionary<string, FeatureFlag> _featuresByKey = new(StringComparer.OrdinalIgnoreCase);
  private readonly Dictionary<(Guid FeatureId, OverrideType Type, string TargetId), bool> _overrides = new();

  public TestFeatureFlagStore AddFeature(FeatureFlag feature)
  {
    _featuresByKey[feature.Key] = feature;
    return this;
  }

  public TestFeatureFlagStore AddOverride(Guid featureId, OverrideType type, string targetId, bool state)
  {
    // Keep normalization aligned with Core conventions.
    var normalizedTarget = type == OverrideType.Region
        ? RegionCode.Normalize(targetId)
        : OverrideTarget.Normalize(targetId);

    _overrides[(featureId, type, normalizedTarget)] = state;
    return this;
  }

  public bool TryGetFeatureByKey(string normalizedKey, out FeatureFlag feature)
  {
    normalizedKey = FeatureKey.Normalize(normalizedKey);
    return _featuresByKey.TryGetValue(normalizedKey, out feature!);
  }

  public bool TryGetOverride(Guid featureFlagId, OverrideType type, string normalizedTargetId, out bool state)
      => _overrides.TryGetValue((featureFlagId, type, normalizedTargetId), out state);
}
