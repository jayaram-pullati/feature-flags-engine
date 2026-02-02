using Asp.Versioning;
using FeatureFlags.Infrastructure.Stores;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/feature-flags")]
public sealed class FeatureFlagsAdminController(FeatureFlagSnapshotLoader loader, CachedFeatureFlagStore store)
    : ControllerBase
{
  [HttpGet("status")]
  public IActionResult Status()
      => Ok(new { features = store.FeatureCount, overrides = store.OverrideCount });

  [HttpPost("refresh")]
  public async Task<IActionResult> Refresh(CancellationToken ct)
  {
    await loader.LoadAsync(ct);
    return Ok(new { message = "Snapshot refreshed." });
  }
}
