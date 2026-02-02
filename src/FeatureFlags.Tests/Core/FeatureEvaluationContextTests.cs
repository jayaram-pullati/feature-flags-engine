using FeatureFlags.Core.Evaluation;
using FluentAssertions;

namespace FeatureFlags.Tests.Core;

public sealed class FeatureEvaluationContextTests
{
  [Fact]
  public void Ctor_Normalizes_UserId_Region_And_GroupIds()
  {
    // Arrange
    var userId = "  u1  ";
    var groupIds = new[] { " beta ", "BETA", "  ", "admin" };
    var region = "in";

    // Act
    var ctx = new FeatureEvaluationContext(userId: userId, groupIds: groupIds, region: region);

    // Assert
    ctx.UserId.Should().Be("u1");
    ctx.Region.Should().Be("IN");
    ctx.GroupIds.Should().BeEquivalentTo(new[] { "beta", "admin" }, o => o.WithStrictOrdering());
  }

  [Fact]
  public void Ctor_Removes_Empty_And_Duplicate_GroupIds()
  {
    // Arrange
    var groupIds = new[] { "beta", " ", "", "BETA", "admin", "Admin" };

    // Act
    var ctx = new FeatureEvaluationContext(groupIds: groupIds);

    // Assert
    ctx.GroupIds.Should().BeEquivalentTo(new[] { "beta", "admin" }, o => o.WithStrictOrdering());
  }
}
