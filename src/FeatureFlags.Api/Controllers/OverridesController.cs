using Asp.Versioning;
using FeatureFlags.Api.Contracts;
using FeatureFlags.Core.Contracts;
using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Validation;
using FeatureFlags.Infrastructure.Stores;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/{key}/overrides")]
public sealed class OverridesController(
    IFeatureFlagRepository featureRepo,
    IFeatureOverrideRepository overrideRepo,
    IUnitOfWork uow,
    FeatureFlagSnapshotLoader snapshotLoader) : ControllerBase
{
  [HttpPut("user/{userId}")]
  public Task<IActionResult> UpsertUser(string key, string userId, UpsertOverrideRequest request, CancellationToken ct)
      => Upsert(key, OverrideType.User, userId, request.State, ct);

  [HttpPut("group/{groupId}")]
  public Task<IActionResult> UpsertGroup(string key, string groupId, UpsertOverrideRequest request, CancellationToken ct)
      => Upsert(key, OverrideType.Group, groupId, request.State, ct);

  [HttpPut("region/{region}")]
  public Task<IActionResult> UpsertRegion(string key, string region, UpsertOverrideRequest request, CancellationToken ct)
      => Upsert(key, OverrideType.Region, region, request.State, ct);

  [HttpDelete("user/{userId}")]
  public Task<IActionResult> DeleteUser(string key, string userId, CancellationToken ct)
      => Remove(key, OverrideType.User, userId, ct);

  [HttpDelete("group/{groupId}")]
  public Task<IActionResult> DeleteGroup(string key, string groupId, CancellationToken ct)
      => Remove(key, OverrideType.Group, groupId, ct);

  [HttpDelete("region/{region}")]
  public Task<IActionResult> DeleteRegion(string key, string region, CancellationToken ct)
      => Remove(key, OverrideType.Region, region, ct);

  private async Task<IActionResult> Upsert(string key, OverrideType type, string targetId, bool state, CancellationToken ct)
  {
    var normalizedKey = FeatureKey.Normalize(key);
    FeatureKeyValidator.EnsureValid(normalizedKey);

    var feature = await featureRepo.GetByKeyAsync(normalizedKey, ct);
    if (feature is null)
      return FeatureNotFound(normalizedKey);

    var model = new FeatureOverride(
        id: Guid.NewGuid(),
        featureFlagId: feature.Id,
        type: type,
        targetId: targetId,
        state: state
    );

    await overrideRepo.UpsertAsync(model, ct);
    await uow.SaveChangesAsync(ct);

    await snapshotLoader.LoadAsync(ct);

    return Ok(new { message = "Override upserted." });
  }

  private async Task<IActionResult> Remove(string key, OverrideType type, string targetId, CancellationToken ct)
  {
    var normalizedKey = FeatureKey.Normalize(key);
    FeatureKeyValidator.EnsureValid(normalizedKey);

    var feature = await featureRepo.GetByKeyAsync(normalizedKey, ct);
    if (feature is null)
      return FeatureNotFound(normalizedKey);

    await overrideRepo.RemoveAsync(feature.Id, type, targetId, ct);
    await uow.SaveChangesAsync(ct);

    await snapshotLoader.LoadAsync(ct);

    return NoContent();
  }

  private static NotFoundObjectResult FeatureNotFound(string normalizedKey)
  {
    var problem = new ProblemDetails
    {
      Status = StatusCodes.Status404NotFound,
      Title = "Not found",
      Detail = $"Feature '{normalizedKey}' was not found."
    };

    return new NotFoundObjectResult(problem);
  }

}
