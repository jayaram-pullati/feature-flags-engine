using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Validation;
using FeatureFlags.Infrastructure.Persistence.Entities;
using FeatureFlags.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Tests.Infrastructure;

public sealed class FeatureOverrideRepositoryTests
{
  [Fact]
  public async Task UpsertAsync_UpdatesState_WhenOverrideAlreadyExists()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    await using var _ = db;
    await using var __ = connection;

    var featureId = Guid.NewGuid();
    db.FeatureFlags.Add(new FeatureFlagEntity { Id = featureId, Key = "flag-x", DefaultState = false });
    await db.SaveChangesAsync();

    // Create the model FIRST (this normalizes model.TargetId via FeatureOverride ctor)
    var model = new FeatureOverride(
      id: Guid.NewGuid(),
      featureFlagId: featureId,
      type: OverrideType.Group,
      targetId: "BETA",
      state: true
    );

    // Compute EXACT normalizedTarget the repository will use
    var normalizedTarget = model.Type switch
    {
      OverrideType.Region => RegionCode.Normalize(model.TargetId),
      _ => OverrideTarget.Normalize(model.TargetId)
    };

    // Seed existing row with EXACT same key (featureId + type + target)
    db.FeatureOverrides.Add(new FeatureOverrideEntity
    {
      Id = Guid.NewGuid(),
      FeatureFlagId = featureId,
      Type = model.Type,
      TargetId = normalizedTarget,
      State = false
    });

    await db.SaveChangesAsync();

    var repo = new FeatureOverrideRepository(db);

    // Act
    await repo.UpsertAsync(model);
    await db.SaveChangesAsync();

    // Assert: must still be exactly one row total (update, not insert)
    var totalCount = await db.FeatureOverrides.CountAsync();
    totalCount.Should().Be(1);

    var saved = await db.FeatureOverrides.SingleAsync();
    saved.FeatureFlagId.Should().Be(featureId);
    saved.Type.Should().Be(model.Type);
    saved.TargetId.Should().Be(normalizedTarget);
    saved.State.Should().BeTrue();
  }
}
