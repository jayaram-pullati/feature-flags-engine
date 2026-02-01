namespace FeatureFlags.Core.Errors;

public sealed class ValidationException(string message) : DomainException(message)
{
}
