using FeatureFlags.Core.Validation;
using FluentAssertions;

namespace FeatureFlags.Tests.Core.Validation;

public sealed class RegionCodeTests
{
  [Fact]
  public void Normalize_TrimsAndUppercases()
  {
    RegionCode.Normalize("  in  ").Should().Be("IN");
  }
}
