using GoTrexia.Core.ValueObjects;

namespace GoTrexia.Core;

public sealed record StageDefinition(
    string Name,
    string Description,
    string BackgroundImage,
    int HintButtonTimeoutSeconds,
    GeoPoint SearchLocation,
    GeoPoint HintLocation,
    GeoPoint TargetLocation,
    int Score,
    string Answer
);
