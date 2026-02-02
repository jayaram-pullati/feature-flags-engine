using FeatureFlags.Api.Contracts;
using FeatureFlags.Api.Controllers;
using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Evaluation;
using FeatureFlags.Core.Validation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FeatureFlags.Tests.Api;

public sealed class EvaluationControllerTests
{
  [Fact]
  public void Evaluate_ReturnsOk_WithEnabledAndSource_Default()
  {
    // Arrange
    var store = new Mock<IFeatureFlagStore>(MockBehavior.Strict);

    var featureId = Guid.NewGuid();
    var feature = new FeatureFlag(featureId, FeatureKey.Normalize("eval-flag"), defaultState: false, description: null);

    store.Setup(x => x.TryGetFeatureByKey(It.IsAny<string>(), out feature))
        .Returns(true);

    store.Setup(x => x.TryGetOverride(It.IsAny<Guid>(), It.IsAny<OverrideType>(), It.IsAny<string>(), out It.Ref<bool>.IsAny))
        .Returns(false);

    var evaluator = new FeatureFlagEvaluator(store.Object);
    var sut = new EvaluationController(evaluator)
    {
      ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
    };

    var request = new EvaluateFeatureRequest(
      UserId: null,
      GroupIds: Array.Empty<string>(),
      Region: "IN"
    );

    // Act
    var result = sut.Evaluate("eval-flag", request);

    // Assert
    var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
    var body = ok.Value.Should().BeOfType<EvaluateFeatureResponse>().Subject;

    body.Enabled.Should().BeFalse();
    body.Source.Should().Be("Default");
  }
}
