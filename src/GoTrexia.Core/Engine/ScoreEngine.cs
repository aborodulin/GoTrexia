using GoTrexia.Core.ValueObjects;

namespace GoTrexia.Core.Engine;

public sealed class ScoreEngine
{
    public StageScoreResult Calculate(int basePoints, bool hintUsed, bool skipped)
    {
        var earnedPoints = skipped
            ? 0
            : hintUsed
                ? basePoints / 2
                : basePoints;

        return new StageScoreResult(earnedPoints, hintUsed, skipped);
    }
}
