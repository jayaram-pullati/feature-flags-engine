using FeatureFlags.Core.Domain;
using FeatureFlags.Core.Errors;

namespace FeatureFlags.Core.Validation;

public static class OverrideTargetValidator
{
  public static void EnsureValid(OverrideType type, string normalizedTargetId)
  {
    if (string.IsNullOrWhiteSpace(normalizedTargetId))
      throw new ValidationException("Override targetId is required.");

    if (normalizedTargetId.Length > 200)
      throw new ValidationException("Override targetId is too long (max 200).");

    // Basic type-specific guidance (kept lightweight)
    if (type == OverrideType.Region)
    {
      // Use ISO-like uppercase region codes: IN, US, EU-WEST, etc.
      // We normalize region separately in context, but overrides can still be stored as-is.
      // Infrastructure can choose to normalize for region overrides.
    }
  }
}
