using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Errors;
using FeatureFlags.Core.Validation;
using FeatureFlags.Infrastructure.Repositories;
using FluentAssertions;

namespace FeatureFlags.Tests.Infrastructure;

public sealed class FeatureFlagRepositoryTests
{
  [Fact]
  public async Task AddAsync_PersistsFeature_AndGetByKeyAsyncReturnsDomain()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    await using var _ = db;
    await using var __ = connection;

    var repo = new FeatureFlagRepository(db);
    var feature = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("NewSearch"), defaultState: true, description: "desc");

    // Act
    await repo.AddAsync(feature);
    await db.SaveChangesAsync();

    var loaded = await repo.GetByKeyAsync("newsearch"); // case-insensitive normalize

    // Assert
    loaded.Should().NotBeNull();
    loaded!.Key.Should().Be(FeatureKey.Normalize("NewSearch"));
    loaded.DefaultState.Should().BeTrue();
    loaded.Description.Should().Be("desc");
  }

  [Fact]
  public async Task AddAsync_ThrowsValidationException_WhenDuplicateKey()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    await using var _ = db;
    await using var __ = connection;

    var repo = new FeatureFlagRepository(db);

    var f1 = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("dup-flag"), defaultState: false, description: null);
    var f2 = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("DUP-FLAG"), defaultState: true, description: null);

    await repo.AddAsync(f1);
    await db.SaveChangesAsync();

    // Act
    var act = async () =>
    {
      await repo.AddAsync(f2);
      await db.SaveChangesAsync();
    };

    // Assert
    await act.Should().ThrowAsync<ValidationException>()
        .WithMessage("Feature 'dup-flag' already exists.");
  }

  [Fact]
  public async Task UpdateAsync_UpdatesDefaultStateAndDescription()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    await using var _ = db;
    await using var __ = connection;

    var repo = new FeatureFlagRepository(db);

    var feature = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("flag-x"), defaultState: false, description: "old");
    await repo.AddAsync(feature);
    await db.SaveChangesAsync();

    var updated = new FeatureFlag(feature.Id, feature.Key, defaultState: true, description: "new");

    // Act
    await repo.UpdateAsync(updated);
    await db.SaveChangesAsync();

    var loaded = await repo.GetByIdAsync(feature.Id);

    // Assert
    loaded.Should().NotBeNull();
    loaded!.DefaultState.Should().BeTrue();
    loaded.Description.Should().Be("new");
  }

  [Fact]
  public async Task UpdateAsync_ThrowsFeatureNotFound_WhenIdDoesNotExist()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    await using var _ = db;
    await using var __ = connection;

    var repo = new FeatureFlagRepository(db);

    var missing = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("missing"), defaultState: false, description: null);

    // Act
    var act = async () =>
    {
      await repo.UpdateAsync(missing);
      await db.SaveChangesAsync();
    };

    // Assert
    await act.Should().ThrowAsync<FeatureNotFoundException>();
  }

  [Fact]
  public async Task DeleteAsync_RemovesEntity_WhenExists()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    await using var _ = db;
    await using var __ = connection;

    var repo = new FeatureFlagRepository(db);

    var feature = new FeatureFlag(Guid.NewGuid(), FeatureKey.Normalize("flag-del"), defaultState: false, description: null);
    await repo.AddAsync(feature);
    await db.SaveChangesAsync();

    // Act
    await repo.DeleteAsync(feature.Id);
    await db.SaveChangesAsync();

    var loaded = await repo.GetByIdAsync(feature.Id);

    // Assert
    loaded.Should().BeNull();
  }

  [Fact]
  public async Task DeleteAsync_DoesNothing_WhenEntityDoesNotExist()
  {
    // Arrange
    var (db, connection) = TestDbContextFactory.Create();
    await using var _ = db;
    await using var __ = connection;

    var repo = new FeatureFlagRepository(db);

    // Act
    await repo.DeleteAsync(Guid.NewGuid());
    await db.SaveChangesAsync();

    // Assert
    // no throw is success
    true.Should().BeTrue();
  }
}
