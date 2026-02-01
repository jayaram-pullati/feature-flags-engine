namespace FeatureFlags.Core.Domain;

public enum EvaluationSource
{
  Default = 1,
  RegionOverride = 2,
  GroupOverride = 3,
  UserOverride = 4
}
