using FeatureFlags.Core.Domain;

namespace FeatureFlags.Infrastructure.Persistence.Entities;

public sealed class FeatureOverrideEntity
{
  public Guid Id { get; set; }
  public Guid FeatureFlagId { get; set; }
  public OverrideType Type { get; set; }
  public string TargetId { get; set; } = default!;
  public bool State { get; set; }

  public FeatureFlagEntity FeatureFlag { get; set; } = default!;
}
