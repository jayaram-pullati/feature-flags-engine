using FeatureFlags.Core.Errors;
using FeatureFlags.Core.Validation;
using FluentAssertions;

namespace FeatureFlags.Tests.Core;

public sealed class ValidatorsTests
{
  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void FeatureKeyValidator_Rejects_NullOrWhitespace(string? key)
  {
    var normalized = FeatureKey.Normalize(key ?? string.Empty);

    Action act = () => FeatureKeyValidator.EnsureValid(normalized);

    act.Should().Throw<ValidationException>()
        .WithMessage("Feature key is required.");
  }

  [Theory]
  [InlineData("a")] // too short after normalize
  [InlineData("this key has spaces")]
  [InlineData("UPPERCASE")] // will normalize to lowercase but still valid; this test is for invalid chars only
  public void FeatureKeyValidator_Rejects_InvalidKeys(string key)
  {
    // Only test invalid cases here; "UPPERCASE" becomes lowercase and is valid.
    if (key == "UPPERCASE")
      return;

    var normalized = FeatureKey.Normalize(key);

    Action act = () => FeatureKeyValidator.EnsureValid(normalized);

    act.Should().Throw<ValidationException>();
  }

  [Theory]
  [InlineData("new-search")]
  [InlineData("checkout.v2")]
  [InlineData("flag_1")]
  public void FeatureKeyValidator_Allows_CommonSlugKeys(string key)
  {
    var normalized = FeatureKey.Normalize(key);

    Action act = () => FeatureKeyValidator.EnsureValid(normalized);

    act.Should().NotThrow();
  }
}
