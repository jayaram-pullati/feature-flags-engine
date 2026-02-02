using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Errors;
using FeatureFlags.Core.Validation;
using FluentAssertions;

namespace FeatureFlags.Tests.Core.Validation;

public sealed class OverrideTargetTests
{
  [Fact]
  public void EnsureValid_RejectsEmpty()
  {
    Action act = () => OverrideTargetValidator.EnsureValid(OverrideType.User, "");

    act.Should().Throw<ValidationException>();
  }
}
