namespace FeatureFlags.Core.Errors;

public sealed class FeatureNotFoundException(string featureKey) : DomainException($"Feature '{featureKey}' was not found.")
{
}
