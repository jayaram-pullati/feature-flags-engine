using FeatureFlags.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Tests.Infrastructure;

public static class TestDbContextFactory
{
  public static (FeatureFlagsDbContext Db, SqliteConnection Connection) Create()
  {
    var connection = new SqliteConnection("Filename=:memory:");
    connection.Open();

    var options = new DbContextOptionsBuilder<FeatureFlagsDbContext>()
        .UseSqlite(connection)
        .EnableSensitiveDataLogging()
        .Options;

    var db = new FeatureFlagsDbContext(options);
    db.Database.EnsureCreated();

    return (db, connection);
  }
}
