using Asp.Versioning;
using FeatureFlags.Api.Contracts;
using FeatureFlags.Core.Contracts;
using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Validation;
using FeatureFlags.Infrastructure.Persistence;
using FeatureFlags.Infrastructure.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features")]
public sealed class FeaturesController(
    IFeatureFlagRepository featureRepo,
    IUnitOfWork uow,
    FeatureFlagSnapshotLoader snapshotLoader,
    FeatureFlagsDbContext db) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IReadOnlyList<FeatureResponse>>> List(CancellationToken ct)
  {
    var items = await db.FeatureFlags
        .AsNoTracking()
        .OrderBy(f => f.Key)
        .Select(f => new FeatureResponse(f.Key, f.DefaultState, f.Description))
        .ToListAsync(ct);

    return Ok(items);
  }

  [HttpGet("{key}")]
  public async Task<ActionResult<FeatureResponse>> Get(string key, CancellationToken ct)
  {
    var normalizedKey = FeatureKey.Normalize(key);
    FeatureKeyValidator.EnsureValid(normalizedKey);

    var feature = await featureRepo.GetByKeyAsync(normalizedKey, ct);
    if (feature is null)
      return NotFound(new { message = $"Feature '{normalizedKey}' was not found." });

    return Ok(new FeatureResponse(feature.Key, feature.DefaultState, feature.Description));
  }

  [HttpPost]
  public async Task<ActionResult<FeatureResponse>> Create(CreateFeatureRequest request, CancellationToken ct)
  {
    var normalizedKey = FeatureKey.Normalize(request.Key);
    FeatureKeyValidator.EnsureValid(normalizedKey);

    // ✅ Idempotency-by-key:
    // If feature already exists → check if identical → return 200 OK
    // If different → return 409 ProblemDetails
    var existing = await featureRepo.GetByKeyAsync(normalizedKey, ct);
    if (existing is not null)
    {
      var desiredDescription = string.IsNullOrWhiteSpace(request.Description)
          ? null
          : request.Description.Trim();

      var same =
          existing.DefaultState == request.DefaultState &&
          string.Equals(existing.Description, desiredDescription, StringComparison.Ordinal);

      if (same)
      {
        // Idempotent return
        return Ok(new FeatureResponse(existing.Key, existing.DefaultState, existing.Description));
      }

      // ❌ Conflict → return ProblemDetails (consistent with middleware style)
      var problem = new ProblemDetails
      {
        Status = StatusCodes.Status409Conflict,
        Title = "Conflict",
        Detail = $"Feature '{normalizedKey}' already exists with different values. Use PUT to update."
      };

      return Conflict(problem);
    }

    // Normal create flow
    var feature = new FeatureFlag(Guid.NewGuid(), normalizedKey, request.DefaultState, request.Description);

    await featureRepo.AddAsync(feature, ct);
    await uow.SaveChangesAsync(ct);
    await snapshotLoader.LoadAsync(ct);

    return CreatedAtAction(nameof(Get), new { version = "1.0", key = feature.Key },
        new FeatureResponse(feature.Key, feature.DefaultState, feature.Description));
  }

  [HttpPut("{key}")]
  public async Task<ActionResult<FeatureResponse>> Update(string key, UpdateFeatureRequest request, CancellationToken ct)
  {
    var normalizedKey = FeatureKey.Normalize(key);
    FeatureKeyValidator.EnsureValid(normalizedKey);

    var feature = await featureRepo.GetByKeyAsync(normalizedKey, ct);
    if (feature is null)
      return NotFound(new { message = $"Feature '{normalizedKey}' was not found." });

    feature.SetDefaultState(request.DefaultState);
    feature.SetDescription(request.Description);

    await featureRepo.UpdateAsync(feature, ct);
    await uow.SaveChangesAsync(ct);

    await snapshotLoader.LoadAsync(ct);

    return Ok(new FeatureResponse(feature.Key, feature.DefaultState, feature.Description));
  }

  [HttpDelete("{key}")]
  public async Task<IActionResult> Delete(string key, CancellationToken ct)
  {
    var normalizedKey = FeatureKey.Normalize(key);
    FeatureKeyValidator.EnsureValid(normalizedKey);

    var feature = await featureRepo.GetByKeyAsync(normalizedKey, ct);
    if (feature is null)
      return NotFound(new { message = $"Feature '{normalizedKey}' was not found." });

    await featureRepo.DeleteAsync(feature.Id, ct);
    await uow.SaveChangesAsync(ct);

    await snapshotLoader.LoadAsync(ct);

    return NoContent();
  }
}
