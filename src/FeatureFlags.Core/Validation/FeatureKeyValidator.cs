
using FeatureFlags.Core.Errors;
using System.Text.RegularExpressions;

namespace FeatureFlags.Core.Validation;

public static class FeatureKeyValidator
{
  // Allows: letters, digits, dot, underscore, hyphen (common “slug” style)
  // Examples: "new-search", "checkout.v2", "flag_1"
  private static readonly Regex Allowed = new("^[a-z0-9._-]+$", RegexOptions.Compiled);

  public static void EnsureValid(string normalizedKey)
  {
    if (string.IsNullOrWhiteSpace(normalizedKey))
      throw new ValidationException("Feature key is required.");

    if (normalizedKey.Length is < 2 or > 100)
      throw new ValidationException("Feature key length must be between 2 and 100 characters.");

    if (!Allowed.IsMatch(normalizedKey))
      throw new ValidationException("Feature key contains invalid characters. Allowed: a-z, 0-9, '.', '_', '-'");
  }
}
