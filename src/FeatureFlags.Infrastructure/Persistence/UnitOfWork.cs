using FeatureFlags.Core.Contracts;

namespace FeatureFlags.Infrastructure.Persistence;

public sealed class UnitOfWork(FeatureFlagsDbContext db) : IUnitOfWork
{
  public Task<int> SaveChangesAsync(CancellationToken ct = default)
      => db.SaveChangesAsync(ct);
}
