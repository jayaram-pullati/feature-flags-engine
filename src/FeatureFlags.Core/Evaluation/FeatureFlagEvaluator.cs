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
      => throw new NotImplementedException("Implement in next commit (commit 4).");
}
