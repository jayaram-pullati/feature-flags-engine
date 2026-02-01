using FeatureFlags.Infrastructure.Stores;
using FeatureFlags.Core.Evaluation;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.Infrastructure.DI;

public static partial class DependencyInjection
{
  public static IServiceCollection AddFeatureFlagStore(this IServiceCollection services)
  {
    // Singleton because evaluator reads from it frequently
    services.AddSingleton<CachedFeatureFlagStore>();

    // Loader is scoped because it depends on DbContext
    services.AddScoped<FeatureFlagSnapshotLoader>();

    // IFeatureFlagStore abstraction used by evaluator
    services.AddSingleton<IFeatureFlagStore>(sp =>
        sp.GetRequiredService<CachedFeatureFlagStore>());

    return services;
  }
}
