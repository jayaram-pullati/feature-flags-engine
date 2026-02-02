using FeatureFlags.Core.Errors;
using FeatureFlags.Core.Validation;
using FluentAssertions;

namespace FeatureFlags.Tests.Core.Validation;

public sealed class FeatureKeyTests
{
  [Fact]
  public void Normalize_ConvertsToLowercase_AndTrims()
  {
    FeatureKey.Normalize("  New-Search  ")
        .Should().Be("new-search");
  }

  [Fact]
  public void EnsureValid_Throws_WhenKeyIsNullOrEmpty()
  {
    Action act1 = () => FeatureKeyValidator.EnsureValid("");
    Action act2 = () => FeatureKeyValidator.EnsureValid("   ");

    act1.Should().Throw<ValidationException>();
    act2.Should().Throw<ValidationException>();
  }

  [Fact]
  public void EnsureValid_Throws_WhenKeyHasInvalidCharacters()
  {
    Action act = () => FeatureKeyValidator.EnsureValid("bad key!*");

    act.Should().Throw<ValidationException>();
  }
}
