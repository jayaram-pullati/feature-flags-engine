namespace FeatureFlags.Core.Validation;

public static class FeatureKey
{
  public static string Normalize(string key)
      => (key ?? string.Empty).Trim().ToLowerInvariant();
}
