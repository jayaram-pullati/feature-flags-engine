using FeatureFlags.Api.Contracts;
using FeatureFlags.Api.Controllers;
using FeatureFlags.Core.Contracts;
using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Validation;
using FeatureFlags.Infrastructure.Persistence;
using FeatureFlags.Infrastructure.Stores;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FeatureFlags.Tests.Api;

public sealed class OverridesControllerTests
{
  private static FeatureFlagsDbContext NewInMemoryDb()
  {
    var options = new DbContextOptionsBuilder<FeatureFlagsDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    return new FeatureFlagsDbContext(options);
  }

  private static OverridesController CreateSut(
      Mock<IFeatureFlagRepository> featureRepo,
      Mock<IFeatureOverrideRepository> overrideRepo,
      Mock<IUnitOfWork> uow,
      FeatureFlagSnapshotLoader loader)
  {
    var sut = new OverridesController(featureRepo.Object, overrideRepo.Object, uow.Object, loader)
    {
      ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
    };

    return sut;
  }

  [Fact]
  public async Task UpsertUser_ReturnsNotFoundProblemDetails_WhenFeatureMissing()
  {
    // Arrange
    await using var db = NewInMemoryDb();
    var store = new CachedFeatureFlagStore();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    var featureRepo = new Mock<IFeatureFlagRepository>(MockBehavior.Strict);
    featureRepo.Setup(x => x.GetByKeyAsync(FeatureKey.Normalize("missing"), It.IsAny<CancellationToken>()))
        .ReturnsAsync((FeatureFlag?)null);

    var overrideRepo = new Mock<IFeatureOverrideRepository>(MockBehavior.Strict);
    var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);

    var sut = CreateSut(featureRepo, overrideRepo, uow, loader);

    // Act
    var result = await sut.UpsertUser(
      key: "missing",
      userId: "u1",
      request: new UpsertOverrideRequest(State: true),
      ct: CancellationToken.None
    );

    // Assert
    var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
    var pd = notFound.Value.Should().BeOfType<ProblemDetails>().Subject;

    pd.Status.Should().Be(StatusCodes.Status404NotFound);
    pd.Title.Should().Be("Not found");
    pd.Detail.Should().Contain("was not found");

    featureRepo.VerifyAll();
  }

  [Fact]
  public async Task UpsertGroup_UpsertsOverride_SavesAndRefreshes_ReturnsOk()
  {
    // Arrange
    await using var db = NewInMemoryDb();
    var store = new CachedFeatureFlagStore();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    var feature = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("flag-x"), defaultState: false, description: null);

    var featureRepo = new Mock<IFeatureFlagRepository>(MockBehavior.Strict);
    featureRepo.Setup(x => x.GetByKeyAsync(FeatureKey.Normalize("flag-x"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(feature);

    var overrideRepo = new Mock<IFeatureOverrideRepository>(MockBehavior.Strict);
    overrideRepo.Setup(x => x.UpsertAsync(It.IsAny<FeatureOverride>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);
    uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    var sut = CreateSut(featureRepo, overrideRepo, uow, loader);

    // Act
    var result = await sut.UpsertGroup(
      key: "flag-x",
      groupId: "beta",
      request: new UpsertOverrideRequest(State: true),
      ct: CancellationToken.None
    );

    // Assert
    result.Should().BeOfType<OkObjectResult>();

    featureRepo.VerifyAll();
    overrideRepo.VerifyAll();
    uow.VerifyAll();
  }

  [Fact]
  public async Task DeleteRegion_RemovesOverride_SavesAndRefreshes_ReturnsNoContent()
  {
    // Arrange
    await using var db = NewInMemoryDb();
    var store = new CachedFeatureFlagStore();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    var feature = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("flag-x"), defaultState: false, description: null);

    var featureRepo = new Mock<IFeatureFlagRepository>(MockBehavior.Strict);
    featureRepo.Setup(x => x.GetByKeyAsync(FeatureKey.Normalize("flag-x"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(feature);

    var overrideRepo = new Mock<IFeatureOverrideRepository>(MockBehavior.Strict);
    overrideRepo.Setup(x => x.RemoveAsync(feature.Id, OverrideType.Region, "IN", It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);
    uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    var sut = CreateSut(featureRepo, overrideRepo, uow, loader);

    // Act
    var result = await sut.DeleteRegion(
      key: "flag-x",
      region: "IN",
      ct: CancellationToken.None
    );

    // Assert
    result.Should().BeOfType<NoContentResult>();

    featureRepo.VerifyAll();
    overrideRepo.VerifyAll();
    uow.VerifyAll();
  }
}
