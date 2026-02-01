using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Validation;
using FeatureFlags.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Infrastructure.Stores;

/// <summary>
/// Loads domain models from EF Core entities and normalizes them for caching.
/// </summary>
public sealed class FeatureFlagSnapshotLoader
{
  private readonly FeatureFlagsDbContext _db;
  private readonly CachedFeatureFlagStore _store;

  public FeatureFlagSnapshotLoader(FeatureFlagsDbContext db, CachedFeatureFlagStore store)
  {
    _db = db;
    _store = store;
  }

  public async Task LoadAsync(CancellationToken ct = default)
  {
    var flags = await _db.FeatureFlags
        .Include(f => f.Overrides)
        .AsNoTracking()
        .ToListAsync(ct);

    // Convert to domain objects
    var featureMap = new Dictionary<string, FeatureFlag>(StringComparer.OrdinalIgnoreCase);
    var overrideMap = new Dictionary<(Guid, OverrideType, string), bool>();

    foreach (var f in flags)
    {
      var key = FeatureKey.Normalize(f.Key);

      var domainFeature = new FeatureFlag(
          f.Id,
          key,
          f.DefaultState,
          f.Description
      );

      featureMap[key] = domainFeature;

      foreach (var ov in f.Overrides)
      {
        string targetNormalized = ov.Type switch
        {
          OverrideType.Region => RegionCode.Normalize(ov.TargetId),
          _ => OverrideTarget.Normalize(ov.TargetId)
        };

        overrideMap[(ov.FeatureFlagId, ov.Type, targetNormalized)] = ov.State;
      }
    }

    // Atomically replace snapshot
    _store.ReplaceSnapshot(featureMap, overrideMap);
  }
}
