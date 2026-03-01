namespace GoTrexia.Core.Stages;

public sealed record StageState(
    StageStatus Status,
    double DistanceToTargetMeters,
    bool CanConfirm
);
