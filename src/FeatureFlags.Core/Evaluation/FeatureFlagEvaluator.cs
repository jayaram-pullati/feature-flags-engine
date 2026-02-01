using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Errors;
using FeatureFlags.Core.Validation;

namespace FeatureFlags.Core.Evaluation;

/// <summary>
/// Evaluates whether a feature is enabled for a given context.
/// Precedence: UserOverride > GroupOverride > RegionOverride > Default.
/// </summary>
public sealed class FeatureFlagEvaluator
{
  private readonly IFeatureFlagStore _store;

  public FeatureFlagEvaluator(IFeatureFlagStore store)
      => _store = store ?? throw new ArgumentNullException(nameof(store));

  public EvaluationResult Evaluate(string featureKey, FeatureEvaluationContext context)
  {
    if (context is null)
      throw new ValidationException("Context cannot be null.");

    // Normalize key (case-insensitive keys)
    var normalizedKey = FeatureKey.Normalize(featureKey);
    FeatureKeyValidator.EnsureValid(normalizedKey);

    if (!_store.TryGetFeatureByKey(normalizedKey, out var feature))
      throw new FeatureNotFoundException(normalizedKey);

    // 1) User override (highest precedence)
    if (!string.IsNullOrWhiteSpace(context.UserId))
    {
      var userTarget = OverrideTarget.Normalize(context.UserId);

      if (_store.TryGetOverride(feature.Id, OverrideType.User, userTarget, out var userState))
        return new EvaluationResult(userState, EvaluationSource.UserOverride);
    }

    // 2) Group override (next)
    // Decision: first matching group wins based on context group order
    if (context.GroupIds.Count > 0)
    {
      foreach (var groupId in context.GroupIds)
      {
        var groupTarget = OverrideTarget.Normalize(groupId);

        if (_store.TryGetOverride(feature.Id, OverrideType.Group, groupTarget, out var groupState))
          return new EvaluationResult(groupState, EvaluationSource.GroupOverride);
      }
    }

    // 3) Region override (next)
    if (!string.IsNullOrWhiteSpace(context.Region))
    {
      // Context already normalizes to uppercase, but keep defensive normalization
      var regionTarget = RegionCode.Normalize(context.Region);

      if (_store.TryGetOverride(feature.Id, OverrideType.Region, regionTarget, out var regionState))
        return new EvaluationResult(regionState, EvaluationSource.RegionOverride);
    }

    // 4) Default
    return new EvaluationResult(feature.DefaultState, EvaluationSource.Default);
  }
}
