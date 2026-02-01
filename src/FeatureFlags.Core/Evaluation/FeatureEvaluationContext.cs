using FeatureFlags.Core.Validation;

namespace FeatureFlags.Core.Evaluation;

public sealed class FeatureEvaluationContext
{
  public string? UserId { get; }
  public IReadOnlyList<string> GroupIds { get; }
  public string? Region { get; }

  public FeatureEvaluationContext(string? userId = null, IEnumerable<string>? groupIds = null, string? region = null)
  {
    UserId = string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();
    Region = string.IsNullOrWhiteSpace(region) ? null : RegionCode.Normalize(region);

    var groups = (groupIds ?? Array.Empty<string>())
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Select(x => x.Trim())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    GroupIds = groups;

    ContextValidator.EnsureValid(this);
  }
}
