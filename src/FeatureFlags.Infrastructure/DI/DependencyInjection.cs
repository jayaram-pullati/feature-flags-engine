using FeatureFlags.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.Infrastructure.DI;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDbContext<FeatureFlagsDbContext>(options =>
    {
      options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
    });

    return services;
  }
}
