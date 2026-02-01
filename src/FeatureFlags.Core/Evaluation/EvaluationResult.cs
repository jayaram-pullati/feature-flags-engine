using FeatureFlags.Core.Domain;

namespace FeatureFlags.Core.Evaluation;

public sealed record EvaluationResult(bool Enabled, EvaluationSource Source);
