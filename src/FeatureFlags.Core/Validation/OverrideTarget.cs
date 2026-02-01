namespace FeatureFlags.Core.Validation;

public static class OverrideTarget
{
  public static string Normalize(string targetId)
      => (targetId ?? string.Empty).Trim();
}
