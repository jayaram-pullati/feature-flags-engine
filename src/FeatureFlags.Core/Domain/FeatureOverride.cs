using FeatureFlags.Core.Validation;

namespace FeatureFlags.Core.Domain;

public sealed class FeatureOverride
{
  public Guid Id { get; }
  public Guid FeatureFlagId { get; }
  public OverrideType Type { get; }
  public string TargetId { get; }
  public bool State { get; }

  public FeatureOverride(Guid id, Guid featureFlagId, OverrideType type, string targetId, bool state)
  {
    Id = id == Guid.Empty ? throw new ArgumentException("Id cannot be empty.", nameof(id)) : id;
    FeatureFlagId = featureFlagId == Guid.Empty
        ? throw new ArgumentException("FeatureFlagId cannot be empty.", nameof(featureFlagId))
        : featureFlagId;

    Type = type;
    TargetId = OverrideTarget.Normalize(targetId);
    OverrideTargetValidator.EnsureValid(Type, TargetId);

    State = state;
  }
}
