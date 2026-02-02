namespace FeatureFlags.Api.Contracts;

public sealed record CreateFeatureRequest(
    string Key,
    bool DefaultState,
    string? Description
);

public sealed record UpdateFeatureRequest(
    bool DefaultState,
    string? Description
);

public sealed record FeatureResponse(
    string Key,
    bool DefaultState,
    string? Description
);
