using FeatureFlags.Api.Contracts;
using FeatureFlags.Api.Controllers;
using FeatureFlags.Core.Contracts;
using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Validation;
using FeatureFlags.Infrastructure.Persistence;
using FeatureFlags.Infrastructure.Persistence.Entities;
using FeatureFlags.Infrastructure.Stores;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FeatureFlags.Tests.Api;

public sealed class FeaturesControllerTests
{
  private static FeatureFlagsDbContext NewInMemoryDb()
  {
    var options = new DbContextOptionsBuilder<FeatureFlagsDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    return new FeatureFlagsDbContext(options);
  }

  private static FeaturesController CreateSut(
      Mock<IFeatureFlagRepository> featureRepo,
      Mock<IUnitOfWork> uow,
      FeatureFlagSnapshotLoader loader,
      FeatureFlagsDbContext db)
  {
    var sut = new FeaturesController(featureRepo.Object, uow.Object, loader, db)
    {
      ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
    };

    return sut;
  }

  [Fact]
  public async Task List_ReturnsOk_OrderedByKey()
  {
    // Arrange
    await using var db = NewInMemoryDb();
    db.FeatureFlags.Add(new FeatureFlagEntity { Id = Guid.NewGuid(), Key = "b-flag", DefaultState = false, Description = "b" });
    db.FeatureFlags.Add(new FeatureFlagEntity { Id = Guid.NewGuid(), Key = "a-flag", DefaultState = true, Description = "a" });
    await db.SaveChangesAsync();

    var store = new CachedFeatureFlagStore();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    var featureRepo = new Mock<IFeatureFlagRepository>(MockBehavior.Strict);
    var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);

    var sut = CreateSut(featureRepo, uow, loader, db);

    // Act
    var result = await sut.List(CancellationToken.None);

    // Assert
    var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
    var items = ok.Value.Should().BeAssignableTo<IReadOnlyList<FeatureResponse>>().Subject;

    items.Count.Should().Be(2);
    items[0].Key.Should().Be("a-flag");
    items[1].Key.Should().Be("b-flag");
  }

  [Fact]
  public async Task Get_ReturnsOk_WhenFeatureExists()
  {
    // Arrange
    await using var db = NewInMemoryDb();
    var store = new CachedFeatureFlagStore();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    var existing = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("get-flag"), defaultState: true, description: "desc");

    var featureRepo = new Mock<IFeatureFlagRepository>(MockBehavior.Strict);
    featureRepo.Setup(x => x.GetByKeyAsync(FeatureKey.Normalize("get-flag"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(existing);

    var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);

    var sut = CreateSut(featureRepo, uow, loader, db);

    // Act
    var result = await sut.Get("get-flag", CancellationToken.None);

    // Assert
    var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
    var body = ok.Value.Should().BeOfType<FeatureResponse>().Subject;

    body.Key.Should().Be(existing.Key);
    body.DefaultState.Should().BeTrue();
    body.Description.Should().Be("desc");

    featureRepo.VerifyAll();
  }

  [Fact]
  public async Task Get_ReturnsNotFound_WhenMissing()
  {
    // Arrange
    await using var db = NewInMemoryDb();
    var store = new CachedFeatureFlagStore();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    var featureRepo = new Mock<IFeatureFlagRepository>(MockBehavior.Strict);
    featureRepo.Setup(x => x.GetByKeyAsync(FeatureKey.Normalize("missing"), It.IsAny<CancellationToken>()))
        .ReturnsAsync((FeatureFlag?)null);

    var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);

    var sut = CreateSut(featureRepo, uow, loader, db);

    // Act
    var result = await sut.Get("missing", CancellationToken.None);

    // Assert
    result.Result.Should().BeOfType<NotFoundObjectResult>();

    featureRepo.VerifyAll();
  }

  [Fact]
  public async Task Create_ReturnsCreated_WhenNewFeature()
  {
    // Arrange
    await using var db = NewInMemoryDb();
    var store = new CachedFeatureFlagStore();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    var featureRepo = new Mock<IFeatureFlagRepository>(MockBehavior.Strict);
    featureRepo.Setup(x => x.GetByKeyAsync(FeatureKey.Normalize("new-flag"), It.IsAny<CancellationToken>()))
        .ReturnsAsync((FeatureFlag?)null);
    featureRepo.Setup(x => x.AddAsync(It.IsAny<FeatureFlag>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);
    uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    var sut = CreateSut(featureRepo, uow, loader, db);

    var request = new CreateFeatureRequest(
      Key: "new-flag",
      DefaultState: true,
      Description: "desc"
    );

    // Act
    var result = await sut.Create(request, CancellationToken.None);

    // Assert
    var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
    created.ActionName.Should().Be(nameof(FeaturesController.Get));

    var body = created.Value.Should().BeOfType<FeatureResponse>().Subject;
    body.Key.Should().Be(FeatureKey.Normalize("new-flag"));
    body.DefaultState.Should().BeTrue();
    body.Description.Should().Be("desc");

    featureRepo.VerifyAll();
    uow.VerifyAll();
  }

  [Fact]
  public async Task Create_ReturnsOk_WhenIdempotentSamePayload()
  {
    // Arrange
    await using var db = NewInMemoryDb();
    var store = new CachedFeatureFlagStore();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    var existing = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("same-flag"), defaultState: false, description: "desc");

    var featureRepo = new Mock<IFeatureFlagRepository>(MockBehavior.Strict);
    featureRepo.Setup(x => x.GetByKeyAsync(FeatureKey.Normalize("same-flag"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(existing);

    var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);

    var sut = CreateSut(featureRepo, uow, loader, db);

    var request = new CreateFeatureRequest(
      Key: "same-flag",
      DefaultState: false,
      Description: "desc"
    );

    // Act
    var result = await sut.Create(request, CancellationToken.None);

    // Assert
    var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
    var body = ok.Value.Should().BeOfType<FeatureResponse>().Subject;

    body.Key.Should().Be(existing.Key);
    body.DefaultState.Should().Be(existing.DefaultState);
    body.Description.Should().Be(existing.Description);

    featureRepo.VerifyAll();
  }

  [Fact]
  public async Task Create_ReturnsConflictProblemDetails_WhenSameKeyDifferentPayload()
  {
    // Arrange
    await using var db = NewInMemoryDb();
    var store = new CachedFeatureFlagStore();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    var existing = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("conflict-flag"), defaultState: false, description: "v1");

    var featureRepo = new Mock<IFeatureFlagRepository>(MockBehavior.Strict);
    featureRepo.Setup(x => x.GetByKeyAsync(FeatureKey.Normalize("conflict-flag"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(existing);

    var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);

    var sut = CreateSut(featureRepo, uow, loader, db);

    var request = new CreateFeatureRequest(
      Key: "conflict-flag",
      DefaultState: true,
      Description: "v2"
    );

    // Act
    var result = await sut.Create(request, CancellationToken.None);

    // Assert
    var conflict = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
    var pd = conflict.Value.Should().BeOfType<ProblemDetails>().Subject;

    pd.Status.Should().Be(StatusCodes.Status409Conflict);
    pd.Title.Should().Be("Conflict");
    pd.Detail.Should().Contain("already exists");

    featureRepo.VerifyAll();
  }

  [Fact]
  public async Task Update_ReturnsOk_WhenExists_SavesAndRefreshes()
  {
    // Arrange
    await using var db = NewInMemoryDb();
    var store = new CachedFeatureFlagStore();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    var existing = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("flag-u"), defaultState: false, description: "old");

    var featureRepo = new Mock<IFeatureFlagRepository>(MockBehavior.Strict);
    featureRepo.Setup(x => x.GetByKeyAsync(FeatureKey.Normalize("flag-u"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(existing);
    featureRepo.Setup(x => x.UpdateAsync(It.IsAny<FeatureFlag>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);
    uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    var sut = CreateSut(featureRepo, uow, loader, db);

    var request = new UpdateFeatureRequest(
      DefaultState: true,
      Description: "new"
    );

    // Act
    var result = await sut.Update("flag-u", request, CancellationToken.None);

    // Assert
    var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
    var body = ok.Value.Should().BeOfType<FeatureResponse>().Subject;

    body.Key.Should().Be(existing.Key);
    body.DefaultState.Should().BeTrue();
    body.Description.Should().Be("new");

    featureRepo.VerifyAll();
    uow.VerifyAll();
  }

  [Fact]
  public async Task Delete_ReturnsNoContent_WhenExists_SavesAndRefreshes()
  {
    // Arrange
    await using var db = NewInMemoryDb();
    var store = new CachedFeatureFlagStore();
    var loader = new FeatureFlagSnapshotLoader(db, store);

    var existing = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("flag-d"), defaultState: false, description: null);

    var featureRepo = new Mock<IFeatureFlagRepository>(MockBehavior.Strict);
    featureRepo.Setup(x => x.GetByKeyAsync(FeatureKey.Normalize("flag-d"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(existing);
    featureRepo.Setup(x => x.DeleteAsync(existing.Id, It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);
    uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    var sut = CreateSut(featureRepo, uow, loader, db);

    // Act
    var result = await sut.Delete("flag-d", CancellationToken.None);

    // Assert
    result.Should().BeOfType<NoContentResult>();

    featureRepo.VerifyAll();
    uow.VerifyAll();
  }
}
