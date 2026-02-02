using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FeatureFlags.Api.OpenApi;

public sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
  public void Configure(SwaggerGenOptions options)
  {
    foreach (var desc in provider.ApiVersionDescriptions)
    {
      options.SwaggerDoc(desc.GroupName, new OpenApiInfo
      {
        Title = "Feature Flags API",
        Version = desc.ApiVersion.ToString(),
        Description = "Feature Flag Engine (versioned API)"
      });
    }
  }
}
