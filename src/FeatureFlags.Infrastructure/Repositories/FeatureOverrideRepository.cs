using FeatureFlags.Core.Contracts;
using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Validation;
using FeatureFlags.Infrastructure.Persistence;
using FeatureFlags.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Infrastructure.Repositories;

public sealed class FeatureOverrideRepository(FeatureFlagsDbContext db) : IFeatureOverrideRepository
{
  public async Task UpsertAsync(FeatureOverride model, CancellationToken ct = default)
  {
    // Normalize target ID consistently (required for uniqueness + cache lookups)
    var normalizedTarget = model.Type switch
    {
      OverrideType.Region => RegionCode.Normalize(model.TargetId),
      _ => OverrideTarget.Normalize(model.TargetId)
    };

    var existing = await db.FeatureOverrides
        .FirstOrDefaultAsync(o =>
            o.FeatureFlagId == model.FeatureFlagId &&
            o.Type == model.Type &&
            o.TargetId == normalizedTarget, ct);

    if (existing is null)
    {
      var entity = model.ToEntity();
      entity.TargetId = normalizedTarget;
      db.FeatureOverrides.Add(entity);
    }
    else
    {
      existing.State = model.State;
    }
  }

  public async Task RemoveAsync(Guid featureId, OverrideType type, string normalizedTargetId, CancellationToken ct = default)
  {
    var target = type == OverrideType.Region
        ? RegionCode.Normalize(normalizedTargetId)
        : OverrideTarget.Normalize(normalizedTargetId);

    var existing = await db.FeatureOverrides
        .FirstOrDefaultAsync(o =>
            o.FeatureFlagId == featureId &&
            o.Type == type &&
            o.TargetId == target, ct);

    if (existing is null)
      return;

    db.FeatureOverrides.Remove(existing);
  }
}
