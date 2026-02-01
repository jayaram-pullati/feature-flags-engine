using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Evaluation;

namespace FeatureFlags.Infrastructure.Stores;

/// <summary>
/// In-memory snapshot store for fast O(1) feature flag evaluation.
/// This store is populated once at startup and can be refreshed.
/// </summary>
public sealed class CachedFeatureFlagStore : IFeatureFlagStore
{
  private readonly Lock _lock = new();

  // Cached normalized featureKey => FeatureFlag
  private Dictionary<string, FeatureFlag> _features = new(StringComparer.OrdinalIgnoreCase);

  // Cached (featureId, type, normalizedTargetId) => state
  private Dictionary<(Guid FeatureId, OverrideType Type, string TargetId), bool> _overrides
      = [];

  public int FeatureCount => _features.Count;
  public int OverrideCount => _overrides.Count;

  public bool TryGetFeatureByKey(string normalizedKey, out FeatureFlag feature)
      => _features.TryGetValue(normalizedKey, out feature!);

  public bool TryGetOverride(Guid featureFlagId, OverrideType type, string normalizedTargetId, out bool state)
      => _overrides.TryGetValue((featureFlagId, type, normalizedTargetId), out state);

  /// <summary>
  /// Replace the entire snapshot atomically.
  /// </summary>
  public void ReplaceSnapshot(
      IReadOnlyDictionary<string, FeatureFlag> features,
      IReadOnlyDictionary<(Guid, OverrideType, string), bool> overrides)
  {
    lock (_lock)
    {
      _features = new(features, StringComparer.OrdinalIgnoreCase);
      _overrides = new(overrides);
    }
  }
}
