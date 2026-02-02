using FeatureFlags.Api.Controllers;
using FeatureFlags.Infrastructure.Persistence;
using FeatureFlags.Infrastructure.Persistence.Entities;
using FeatureFlags.Infrastructure.Stores;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Tests.Api;

public sealed class FeatureFlagsAdminControllerTests
{
  private static FeatureFlagsDbContext NewInMemoryDb()
  {
    var options = new DbContextOptionsBuilder<FeatureFlagsDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    return new FeatureFlagsDbContext(options);
  }

  [Fact]
  public void Status_ReturnsCounts_FromStore()
  {
    // Arrange
    var store = new CachedFeatureFlagStore();

    using var db = NewInMemoryDb();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    var sut = new FeatureFlagsAdminController(loader, store)
    {
      ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
    };

    // Act
    var result = sut.Status();

    // Assert
    result.Should().BeOfType<OkObjectResult>();
  }

  [Fact]
  public async Task Refresh_LoadsSnapshot_AndReturnsOk()
  {
    // Arrange
    await using var db = NewInMemoryDb();

    // Seed db so loader actually loads something
    db.FeatureFlags.Add(new FeatureFlagEntity
    {
      Id = Guid.NewGuid(),
      Key = "new-search",
      DefaultState = true,
      Description = "desc"
    });

    await db.SaveChangesAsync();

    var store = new CachedFeatureFlagStore();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    var sut = new FeatureFlagsAdminController(loader, store)
    {
      ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
    };

    // Act
    var result = await sut.Refresh(CancellationToken.None);

    // Assert
    result.Should().BeOfType<OkObjectResult>();
    store.FeatureCount.Should().Be(1);
  }
}
