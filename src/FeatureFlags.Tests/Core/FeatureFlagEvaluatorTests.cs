using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Errors;
using FeatureFlags.Core.Evaluation;
using FeatureFlags.Core.Validation;
using FluentAssertions;

namespace FeatureFlags.Tests.Core;

public sealed class FeatureFlagEvaluatorTests
{
  [Fact]
  public void Evaluate_ThrowsValidationException_WhenContextIsNull()
  {
    // Arrange
    var store = new TestFeatureFlagStore().AddFeature("new-search", defaultState: false);
    var sut = new FeatureFlagEvaluator(store);

    // Act
    Action act = () => sut.Evaluate("new-search", null!);

    // Assert
    act.Should().Throw<ValidationException>()
        .WithMessage("Context cannot be null.");
  }

  [Fact]
  public void Evaluate_ReturnsDefault_WhenNoOverridesExist()
  {
    // Arrange
    var store = new TestFeatureFlagStore().AddFeature("NewSearch", defaultState: false);
    var sut = new FeatureFlagEvaluator(store);
    var ctx = new FeatureEvaluationContext(userId: null, groupIds: null, region: null);

    // Act
    var result = sut.Evaluate("NewSearch", ctx);

    // Assert
    result.Enabled.Should().BeFalse();
    result.Source.Should().Be(EvaluationSource.Default);
  }

  [Fact]
  public void Evaluate_UsesUserOverride_WhenUserOverrideExists()
  {
    // Arrange
    var store = new TestFeatureFlagStore()
        .AddFeature("CheckoutV2", defaultState: false)
        .AddOverride("CheckoutV2", OverrideType.User, OverrideTarget.Normalize("u123"), state: true);

    var sut = new FeatureFlagEvaluator(store);
    var ctx = new FeatureEvaluationContext(userId: "u123", groupIds: new[] { "beta" }, region: "IN");

    // Act
    var result = sut.Evaluate("CheckoutV2", ctx);

    // Assert
    result.Enabled.Should().BeTrue();
    result.Source.Should().Be(EvaluationSource.UserOverride);
  }

  [Fact]
  public void Evaluate_UserOverride_WinsOverGroupOverride()
  {
    // Arrange
    var store = new TestFeatureFlagStore()
        .AddFeature("CheckoutV2", defaultState: true)
        .AddOverride("CheckoutV2", OverrideType.User, OverrideTarget.Normalize("u123"), state: false)
        .AddOverride("CheckoutV2", OverrideType.Group, OverrideTarget.Normalize("beta"), state: true);

    var sut = new FeatureFlagEvaluator(store);
    var ctx = new FeatureEvaluationContext(userId: "u123", groupIds: new[] { "beta" }, region: null);

    // Act
    var result = sut.Evaluate("CheckoutV2", ctx);

    // Assert
    result.Enabled.Should().BeFalse();
    result.Source.Should().Be(EvaluationSource.UserOverride);
  }

  [Fact]
  public void Evaluate_UsesFirstMatchingGroupOverride_ByContextOrder()
  {
    // Arrange
    var store = new TestFeatureFlagStore()
        .AddFeature("NewSearch", defaultState: false)
        .AddOverride("NewSearch", OverrideType.Group, OverrideTarget.Normalize("beta"), state: false)
        .AddOverride("NewSearch", OverrideType.Group, OverrideTarget.Normalize("admin"), state: true);

    var sut = new FeatureFlagEvaluator(store);
    var ctx = new FeatureEvaluationContext(userId: null, groupIds: new[] { "beta", "admin" }, region: null);

    // Act
    var result = sut.Evaluate("NewSearch", ctx);

    // Assert
    result.Enabled.Should().BeFalse();
    result.Source.Should().Be(EvaluationSource.GroupOverride);
  }

  [Fact]
  public void Evaluate_GroupOverride_WinsOverRegionOverride_WhenNoUserOverride()
  {
    // Arrange
    var store = new TestFeatureFlagStore()
        .AddFeature("NewSearch", defaultState: false)
        .AddOverride("NewSearch", OverrideType.Group, OverrideTarget.Normalize("beta"), state: true)
        .AddOverride("NewSearch", OverrideType.Region, RegionCode.Normalize("IN"), state: false);

    var sut = new FeatureFlagEvaluator(store);
    var ctx = new FeatureEvaluationContext(userId: null, groupIds: new[] { "beta" }, region: "IN");

    // Act
    var result = sut.Evaluate("NewSearch", ctx);

    // Assert
    result.Enabled.Should().BeTrue();
    result.Source.Should().Be(EvaluationSource.GroupOverride);
  }

  [Fact]
  public void Evaluate_UsesRegionOverride_WhenNoUserOrGroupOverride()
  {
    // Arrange
    var store = new TestFeatureFlagStore()
        .AddFeature("NewSearch", defaultState: false)
        .AddOverride("NewSearch", OverrideType.Region, RegionCode.Normalize("IN"), state: true);

    var sut = new FeatureFlagEvaluator(store);
    var ctx = new FeatureEvaluationContext(userId: null, groupIds: null, region: "in");

    // Act
    var result = sut.Evaluate("NewSearch", ctx);

    // Assert
    result.Enabled.Should().BeTrue();
    result.Source.Should().Be(EvaluationSource.RegionOverride);
  }

  [Fact]
  public void Evaluate_ThrowsFeatureNotFound_WhenFeatureDoesNotExist()
  {
    // Arrange
    var store = new TestFeatureFlagStore();
    var sut = new FeatureFlagEvaluator(store);
    var ctx = new FeatureEvaluationContext();

    // Act
    Action act = () => sut.Evaluate("does-not-exist", ctx);

    // Assert
    act.Should().Throw<FeatureNotFoundException>()
        .WithMessage("Feature 'does-not-exist' was not found.");
  }

  [Fact]
  public void Evaluate_TreatsFeatureKeyCaseInsensitively()
  {
    // Arrange
    var store = new TestFeatureFlagStore().AddFeature("NewSearch", defaultState: true);
    var sut = new FeatureFlagEvaluator(store);
    var ctx = new FeatureEvaluationContext();

    // Act
    var result = sut.Evaluate("NEWSEARCH", ctx);

    // Assert
    result.Enabled.Should().BeTrue();
    result.Source.Should().Be(EvaluationSource.Default);
  }
}
