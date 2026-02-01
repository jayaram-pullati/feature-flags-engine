namespace FeatureFlags.Core.Validation;

public static class RegionCode
{
  public static string Normalize(string region)
      => (region ?? string.Empty).Trim().ToUpperInvariant();
}
