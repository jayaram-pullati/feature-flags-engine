using FeatureFlags.Core.Validation;

namespace FeatureFlags.Core.Domain;

public sealed class FeatureFlag
{
  public Guid Id { get; }
  public string Key { get; }
  public bool DefaultState { get; private set; }
  public string? Description { get; private set; }

  public FeatureFlag(Guid id, string key, bool defaultState, string? description = null)
  {
    Id = id == Guid.Empty ? throw new ArgumentException("Id cannot be empty.", nameof(id)) : id;
    Key = FeatureKey.Normalize(key);
    FeatureKeyValidator.EnsureValid(Key);

    DefaultState = defaultState;
    Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
  }

  public void UpdateDefaultState(bool newState) => DefaultState = newState;

  public void UpdateDescription(string? description)
      => Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
