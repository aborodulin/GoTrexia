using GoTrexia.Core.Stages;
using GoTrexia.Core.ValueObjects;

namespace GoTrexia.Core.Engine;

public sealed class StageCompletionEngine
{
    private readonly DistanceCalculator _distanceCalculator;

    public StageCompletionEngine(DistanceCalculator distanceCalculator)
    {
        _distanceCalculator = distanceCalculator;
    }

    public StageState UpdatePosition(
        GeoPoint player,
        GeoPoint target,
        StageSettings settings)
    {
        var distance = _distanceCalculator.Calculate(player, target);

        if (distance <= settings.ConfirmationRadiusMeters)
        {
            return new StageState(
                StageStatus.AwaitingConfirmation,
                distance,
                true
            );
        }

        return new StageState(
            StageStatus.InProgress,
            distance,
            false
        );
    }

    public StageState ConfirmFound()
        => new(StageStatus.Completed, 0, false);

    public StageState Skip()
        => new(StageStatus.Skipped, 0, false);
}
