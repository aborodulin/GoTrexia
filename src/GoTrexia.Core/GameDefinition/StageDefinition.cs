using GoTrexia.Core.ValueObjects;

namespace GoTrexia.Core;

public sealed record StageDefinition(
    string Name,
    string Description,
    string BackgroundImage,
    GeoPoint TargetLocation,
    int Score
);
