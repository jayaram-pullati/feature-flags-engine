using FeatureFlags.Core.Domain;

namespace FeatureFlags.Core.Contracts;

public interface IFeatureFlagRepository
{
  Task<FeatureFlag?> GetByKeyAsync(string normalizedKey, CancellationToken ct = default);
  Task<FeatureFlag?> GetByIdAsync(Guid id, CancellationToken ct = default);

  Task AddAsync(FeatureFlag feature, CancellationToken ct = default);
  Task UpdateAsync(FeatureFlag feature, CancellationToken ct = default);
  Task DeleteAsync(Guid id, CancellationToken ct = default);
}
