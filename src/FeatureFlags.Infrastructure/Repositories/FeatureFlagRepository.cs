using FeatureFlags.Core.Contracts;
using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Errors;
using FeatureFlags.Core.Validation;
using FeatureFlags.Infrastructure.Persistence;
using FeatureFlags.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Infrastructure.Repositories;

public sealed class FeatureFlagRepository(FeatureFlagsDbContext db) : IFeatureFlagRepository
{
  public async Task<FeatureFlag?> GetByKeyAsync(string normalizedKey, CancellationToken ct = default)
  {
    var key = FeatureKey.Normalize(normalizedKey);

    var entity = await db.FeatureFlags
        .AsNoTracking()
        .FirstOrDefaultAsync(f => f.Key == key, ct);

    return entity?.ToDomain();
  }

  public async Task<FeatureFlag?> GetByIdAsync(Guid id, CancellationToken ct = default)
  {
    var entity = await db.FeatureFlags
        .AsNoTracking()
        .FirstOrDefaultAsync(f => f.Id == id, ct);

    return entity?.ToDomain();
  }

  public async Task AddAsync(FeatureFlag feature, CancellationToken ct = default)
  {
    // Feature key is expected normalized in domain, but normalize defensively
    var key = FeatureKey.Normalize(feature.Key);
    FeatureKeyValidator.EnsureValid(key);

    var exists = await db.FeatureFlags.AnyAsync(f => f.Key == key, ct);
    if (exists)
      throw new ValidationException($"Feature '{key}' already exists.");

    var entity = feature.ToEntity();
    entity.Key = key;
    db.FeatureFlags.Add(entity);
  }

  public async Task UpdateAsync(FeatureFlag feature, CancellationToken ct = default)
  {
    var entity = await db.FeatureFlags.FirstOrDefaultAsync(f => f.Id == feature.Id, ct);
    if (entity is null)
      throw new FeatureNotFoundException(feature.Key);

    entity.DefaultState = feature.DefaultState;
    entity.Description = feature.Description;
  }

  public async Task DeleteAsync(Guid id, CancellationToken ct = default)
  {
    var entity = await db.FeatureFlags.FirstOrDefaultAsync(f => f.Id == id, ct);
    if (entity is null)
      return;

    db.FeatureFlags.Remove(entity);
  }
}
