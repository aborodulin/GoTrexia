using GoTrexia.Core;
using GoTrexia.Core.Engine;
using GoTrexia.Core.Stages;
using GoTrexia.Core.ValueObjects;

public sealed class GameEngine
{
    private readonly GameDefinition _definition;
    private readonly StageCompletionEngine _stageCompletionEngine;

    private int _currentStageIndex = 0;
    private int _totalScore = 0;
    private bool _hintUsed = false;

    private StageState _currentStageState =
        new(StageStatus.NotStarted, double.MaxValue, false);

    public GameEngine(
        GameDefinition definition,
        StageCompletionEngine stageCompletionEngine)
    {
        _definition = definition;
        _stageCompletionEngine = stageCompletionEngine;
    }

    public StageDefinition CurrentStage
        => _definition.Stages[_currentStageIndex];

    public StageState CurrentStageState => _currentStageState;

    public ScreenDefinition EndScreen => _definition.EndScreen;

    public int TotalScore => _totalScore;

    public bool IsFinished
        => _currentStageIndex >= _definition.Stages.Count;

    public StageState UpdatePlayerPosition(GeoPoint playerLocation)
    {
        var settings = new StageSettings(
            _definition.Settings.MinConfirmationRadiusMeters);

        _currentStageState =
            _stageCompletionEngine.UpdatePosition(
                playerLocation,
                CurrentStage.TargetLocation,
                settings);

        return _currentStageState;
    }

    public void UseHint()
    {
        _hintUsed = true;
    }

    public void ConfirmFound()
    {
        if (_currentStageState.Status != StageStatus.AwaitingConfirmation)
            return;

        var score = CurrentStage.Score;

        if (_hintUsed)
            score /= 2;

        _totalScore += score;

        MoveToNextStage();
    }

    public void Skip()
    {
        MoveToNextStage();
    }

    private void MoveToNextStage()
    {
        _currentStageIndex++;
        _hintUsed = false;
        _currentStageState =
            new(StageStatus.NotStarted, double.MaxValue, false);
    }
}