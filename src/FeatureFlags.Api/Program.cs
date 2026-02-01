using FeatureFlags.Infrastructure.DI;
using FeatureFlags.Infrastructure.Stores;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger/OpenAPI (controller friendly)
builder.Services.AddSwaggerGen(options =>
{
    // XML docs support
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFeatureFlagStore();
builder.Services.AddRepositories();

var app = builder.Build();

// Load feature flags into cache
using (var scope = app.Services.CreateScope())
{
  var loader = scope.ServiceProvider.GetRequiredService<FeatureFlagSnapshotLoader>();
  await loader.LoadAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

await app.RunAsync();
