namespace GoTrexia.Core;

public sealed record GameSettings(
    double MinConfirmationRadiusMeters,
    double MaxSearchRadiusMeters,
    double HintRadiusMeters,
    int HintButtonTimeoutSeconds
);
