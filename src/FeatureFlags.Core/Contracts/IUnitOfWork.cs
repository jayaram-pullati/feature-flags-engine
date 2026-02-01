namespace FeatureFlags.Core.Contracts;

public interface IUnitOfWork
{
  Task<int> SaveChangesAsync(CancellationToken ct = default);
}
