using FeatureFlags.Core.Errors;
using FeatureFlags.Core.Evaluation;

namespace FeatureFlags.Core.Validation;

public static class ContextValidator
{
  public static void EnsureValid(FeatureEvaluationContext context)
  {
    if (context is null) throw new ValidationException("Context cannot be null.");

    if (context.UserId is { Length: > 200 })
      throw new ValidationException("UserId is too long (max 200).");

    if (context.GroupIds.Count > 100)
      throw new ValidationException("Too many groups provided (max 100).");

    foreach (var g in context.GroupIds)
    {
      if (g.Length > 200)
        throw new ValidationException("A groupId is too long (max 200).");
    }

    if (context.Region is { Length: > 50 })
      throw new ValidationException("Region is too long (max 50).");
  }
}
