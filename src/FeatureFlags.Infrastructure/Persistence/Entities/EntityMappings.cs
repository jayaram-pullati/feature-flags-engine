using FeatureFlags.Core.Domain;

namespace FeatureFlags.Infrastructure.Persistence.Entities;

public static class EntityMappings
{
  public static FeatureFlag ToDomain(this FeatureFlagEntity e)
      => new(e.Id, e.Key, e.DefaultState, e.Description);

  public static FeatureFlagEntity ToEntity(this FeatureFlag model)
      => new()
      {
        Id = model.Id,
        Key = model.Key,
        DefaultState = model.DefaultState,
        Description = model.Description
      };

  public static FeatureOverrideEntity ToEntity(this FeatureOverride model)
      => new()
      {
        Id = model.Id,
        FeatureFlagId = model.FeatureFlagId,
        Type = model.Type,
        TargetId = model.TargetId,   
        State = model.State
      };
}
