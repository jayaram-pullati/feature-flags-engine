namespace FeatureFlags.Api.Contracts;

public sealed record EvaluateFeatureRequest(
    string? UserId,
    IReadOnlyList<string>? GroupIds,
    string? Region
);

public sealed record EvaluateFeatureResponse(
    bool Enabled,
    string Source
);
