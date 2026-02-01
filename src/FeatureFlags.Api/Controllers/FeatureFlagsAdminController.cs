using FeatureFlags.Infrastructure.Stores;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Api.Controllers;

[ApiController]
[Route("admin/feature-flags")]
public sealed class FeatureFlagsAdminController : ControllerBase
{
  private readonly FeatureFlagSnapshotLoader _loader;

  public FeatureFlagsAdminController(FeatureFlagSnapshotLoader loader)
  {
    _loader = loader;
  }

  [HttpGet("status")]
  public IActionResult Status([FromServices] CachedFeatureFlagStore store)
  {
    return Ok(new
    {
      features = store.FeatureCount,
      overrides = store.OverrideCount
    });
  }

  [HttpPost("refresh")]
  public async Task<IActionResult> Refresh()
  {
    await _loader.LoadAsync();
    return Ok(new { message = "Snapshot refreshed." });
  }
}
