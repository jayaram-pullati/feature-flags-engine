namespace FeatureFlags.Infrastructure.Persistence.Entities;

public sealed class FeatureFlagEntity
{
  public Guid Id { get; set; }
  public string Key { get; set; } = default!;
  public bool DefaultState { get; set; }
  public string? Description { get; set; }

  public ICollection<FeatureOverrideEntity> Overrides { get; set; } = new List<FeatureOverrideEntity>();
}
