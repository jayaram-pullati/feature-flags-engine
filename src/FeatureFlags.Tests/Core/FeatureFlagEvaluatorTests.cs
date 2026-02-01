using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Errors;
using FeatureFlags.Core.Evaluation;
using FluentAssertions;

namespace FeatureFlags.Tests.Core;

public sealed class FeatureFlagEvaluatorTests
{
  private static FeatureFlag NewFeature(string key, bool defaultState)
      => new(Guid.NewGuid(), key, defaultState, "test");

  [Fact]
  public void IsEnabled_ReturnsDefault_WhenNoOverridesExist()
  {
    // Arrange
    var feature = NewFeature("NewSearch", defaultState: false);
    var store = new TestFeatureFlagStore().AddFeature(feature);
    var ctx = new FeatureEvaluationContext(userId: null, groupIds: null, region: null);

    var sut = new FeatureFlagEvaluator(store);

    // Act
    var result = sut.Evaluate(feature.Key, ctx);

    // Assert
    result.Enabled.Should().BeFalse();
    result.Source.Should().Be(EvaluationSource.Default);
  }

  [Fact]
  public void IsEnabled_UsesUserOverride_WhenUserOverrideExists()
  {
    var feature = NewFeature("CheckoutV2", defaultState: false);
    var store = new TestFeatureFlagStore()
        .AddFeature(feature)
        .AddOverride(feature.Id, OverrideType.User, "u123", state: true);

    var ctx = new FeatureEvaluationContext(userId: "u123", groupIds: new[] { "beta" }, region: "IN");
    var sut = new FeatureFlagEvaluator(store);

    var result = sut.Evaluate(feature.Key, ctx);

    result.Enabled.Should().BeTrue();
    result.Source.Should().Be(EvaluationSource.UserOverride);
  }

  [Fact]
  public void IsEnabled_UserOverride_WinsOverGroupOverride()
  {
    var feature = NewFeature("CheckoutV2", defaultState: true);
    var store = new TestFeatureFlagStore()
        .AddFeature(feature)
        .AddOverride(feature.Id, OverrideType.User, "u123", state: false)
        .AddOverride(feature.Id, OverrideType.Group, "beta", state: true);

    var ctx = new FeatureEvaluationContext(userId: "u123", groupIds: new[] { "beta" }, region: null);
    var sut = new FeatureFlagEvaluator(store);

    var result = sut.Evaluate(feature.Key, ctx);

    result.Enabled.Should().BeFalse();
    result.Source.Should().Be(EvaluationSource.UserOverride);
  }

  [Fact]
  public void IsEnabled_UsesFirstMatchingGroupOverride_ByContextOrder()
  {
    // Decision: first match wins based on context group order.
    // This keeps behaviour deterministic without needing group priority tables.

    var feature = NewFeature("NewSearch", defaultState: false);
    var store = new TestFeatureFlagStore()
        .AddFeature(feature)
        .AddOverride(feature.Id, OverrideType.Group, "beta", state: false)
        .AddOverride(feature.Id, OverrideType.Group, "admin", state: true);

    // beta comes first => should win (false)
    var ctx = new FeatureEvaluationContext(userId: null, groupIds: new[] { "beta", "admin" }, region: null);
    var sut = new FeatureFlagEvaluator(store);

    var result = sut.Evaluate(feature.Key, ctx);

    result.Enabled.Should().BeFalse();
    result.Source.Should().Be(EvaluationSource.GroupOverride);
  }

  [Fact]
  public void IsEnabled_GroupOverride_WinsOverRegionOverride_WhenNoUserOverride()
  {
    var feature = NewFeature("NewSearch", defaultState: false);
    var store = new TestFeatureFlagStore()
        .AddFeature(feature)
        .AddOverride(feature.Id, OverrideType.Group, "beta", state: true)
        .AddOverride(feature.Id, OverrideType.Region, "IN", state: false);

    var ctx = new FeatureEvaluationContext(userId: null, groupIds: new[] { "beta" }, region: "IN");
    var sut = new FeatureFlagEvaluator(store);

    var result = sut.Evaluate(feature.Key, ctx);

    result.Enabled.Should().BeTrue();
    result.Source.Should().Be(EvaluationSource.GroupOverride);
  }

  [Fact]
  public void IsEnabled_UsesRegionOverride_WhenNoUserOrGroupOverride()
  {
    var feature = NewFeature("NewSearch", defaultState: false);
    var store = new TestFeatureFlagStore()
        .AddFeature(feature)
        .AddOverride(feature.Id, OverrideType.Region, "IN", state: true);

    var ctx = new FeatureEvaluationContext(userId: null, groupIds: null, region: "in"); // lower case input
    var sut = new FeatureFlagEvaluator(store);

    var result = sut.Evaluate(feature.Key, ctx);

    result.Enabled.Should().BeTrue();
    result.Source.Should().Be(EvaluationSource.RegionOverride);
  }

  [Fact]
  public void IsEnabled_RegionOverride_DoesNotApply_WhenRegionNotProvided()
  {
    var feature = NewFeature("NewSearch", defaultState: false);
    var store = new TestFeatureFlagStore()
        .AddFeature(feature)
        .AddOverride(feature.Id, OverrideType.Region, "IN", state: true);

    var ctx = new FeatureEvaluationContext(userId: null, groupIds: null, region: null);
    var sut = new FeatureFlagEvaluator(store);

    var result = sut.Evaluate(feature.Key, ctx);

    result.Enabled.Should().BeFalse();
    result.Source.Should().Be(EvaluationSource.Default);
  }

  [Fact]
  public void Evaluate_ThrowsFeatureNotFound_WhenFeatureDoesNotExist()
  {
    var store = new TestFeatureFlagStore();
    var ctx = new FeatureEvaluationContext();

    var sut = new FeatureFlagEvaluator(store);

    Action act = () => sut.Evaluate("does-not-exist", ctx);

    act.Should().Throw<FeatureNotFoundException>()
        .WithMessage("Feature 'does-not-exist' was not found.");
  }

  [Fact]
  public void Evaluate_TreatsFeatureKeyCaseInsensitively()
  {
    var feature = NewFeature("NewSearch", defaultState: true);
    var store = new TestFeatureFlagStore().AddFeature(feature);

    var sut = new FeatureFlagEvaluator(store);
    var ctx = new FeatureEvaluationContext();

    var result = sut.Evaluate("NEWSEARCH", ctx);

    result.Enabled.Should().BeTrue();
    result.Source.Should().Be(EvaluationSource.Default);
  }
}
