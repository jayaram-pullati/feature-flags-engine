using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FeatureFlags.Api.Middleware;
using FeatureFlags.Api.OpenApi;
using FeatureFlags.Core.Evaluation;
using FeatureFlags.Infrastructure.DI;
using FeatureFlags.Infrastructure.Stores;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
  options.DefaultApiVersion = new ApiVersion(1, 0);
  options.AssumeDefaultVersionWhenUnspecified = true;
  options.ReportApiVersions = true;
}).AddMvc();

// Explorer for Swagger (versioned docs)
builder.Services
    .AddApiVersioning(options =>
    {
      options.DefaultApiVersion = new ApiVersion(1, 0);
      options.AssumeDefaultVersionWhenUnspecified = true;
      options.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
      options.GroupNameFormat = "'v'VVV";
      options.SubstituteApiVersionInUrl = true;
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

// App services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFeatureFlagStore();
builder.Services.AddRepositories();
builder.Services.AddSingleton<FeatureFlagEvaluator>();

var app = builder.Build();

// Load feature flags into cache
using (var scope = app.Services.CreateScope())
{
  var loader = scope.ServiceProvider.GetRequiredService<FeatureFlagSnapshotLoader>();
  await loader.LoadAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
  var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    foreach (var desc in provider.ApiVersionDescriptions)
    {
      options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
          $"Feature Flags API {desc.GroupName.ToUpperInvariant()}");
    }
  });
}

app.MapControllers();

await app.RunAsync();
