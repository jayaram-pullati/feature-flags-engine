using FeatureFlags.Core.Domain;

namespace FeatureFlags.Core.Evaluation;

/// <summary>
/// Read-only lookup store used by the evaluator. This is intentionally framework-free.
/// Implementations can be in-memory, cached, DB-backed (but evaluator must not do I/O).
/// </summary>
public interface IFeatureFlagStore
{
  bool TryGetFeatureByKey(string normalizedKey, out FeatureFlag feature);

  bool TryGetOverride(Guid featureFlagId, OverrideType type, string normalizedTargetId, out bool state);
}
