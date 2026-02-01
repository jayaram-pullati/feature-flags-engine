using FeatureFlags.Core.Contracts;
using FeatureFlags.Infrastructure.Persistence;
using FeatureFlags.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.Infrastructure.DI;

public static partial class DependencyInjection
{
  public static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();
    services.AddScoped<IFeatureOverrideRepository, FeatureOverrideRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    return services;
  }
}
