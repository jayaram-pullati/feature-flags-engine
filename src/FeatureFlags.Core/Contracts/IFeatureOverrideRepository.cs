using FeatureFlags.Core.Domain;

namespace FeatureFlags.Core.Contracts;

public interface IFeatureOverrideRepository
{
  Task UpsertAsync(FeatureOverride overrideModel, CancellationToken ct = default);
  Task RemoveAsync(Guid featureId, OverrideType type, string normalizedTargetId, CancellationToken ct = default);
}
