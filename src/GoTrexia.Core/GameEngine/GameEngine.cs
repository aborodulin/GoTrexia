using GoTrexia.Core;
using GoTrexia.Core.Engine;
using GoTrexia.Core.Stages;
using GoTrexia.Core.ValueObjects;

public sealed class GameEngine
{
    private readonly GameDefinition _definition;
    private readonly StageCompletionEngine _stageCompletionEngine;
    private readonly StageStatus[] _stageStatuses;

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
        _stageStatuses = Enumerable.Repeat(StageStatus.NotStarted, _definition.Stages.Count).ToArray();
    }

    public StageDefinition CurrentStage
        => _definition.Stages[_currentStageIndex];

    public int CurrentStageIndex => _currentStageIndex;

    public StageState CurrentStageState => _currentStageState;

    public ScreenDefinition StartScreen => _definition.StartScreen;

    public ScreenDefinition EndScreen => _definition.EndScreen;

    public GameSettings Settings => _definition.Settings;

    public IReadOnlyList<StageDefinition> Stages => _definition.Stages;

    public IReadOnlyList<string> Answers => _definition.Answers;

    public IReadOnlyList<StageCheckpoint> StageCheckpoints =>
        _definition.Stages
            .Select((stage, index) =>
            {
                var status = _stageStatuses[index];
                var isChecked = status is StageStatus.Completed;

                return new StageCheckpoint(
                    stage.Name,
                    stage.Description,
                    stage.Score,
                    isChecked,
                    false);
            })
            .ToList();

    public int TotalScore => _totalScore;

    public bool IsFinished
        => _currentStageIndex >= _definition.Stages.Count;

    public bool IsCurrentStageCompleted =>
        !IsFinished && _stageStatuses[_currentStageIndex] == StageStatus.Completed;

    public bool CanCompleteCurrentStage =>
        !IsFinished && _currentStageState.CanConfirm;

    public bool IsHintUsedForCurrentStage => _hintUsed;

    public StageState UpdatePlayerPosition(GeoPoint playerLocation)
    {
        var settings = new StageSettings(
            CurrentStage.TargetLocation.RadiusMeters);

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

        CompleteCurrentStage();
    }

    public void CompleteCurrentStage()
    {
        if (IsFinished || IsCurrentStageCompleted || !CanCompleteCurrentStage)
            return;

        var score = CurrentStage.Score;

        if (_hintUsed)
            score /= 2;

        CompleteCurrentStage(score);
    }

    public void CompleteCurrentStage(int score)
    {
        if (IsFinished || IsCurrentStageCompleted || !CanCompleteCurrentStage)
            return;

        _totalScore += Math.Max(0, score);
        _stageStatuses[_currentStageIndex] = StageStatus.Completed;

        MoveToNextStage();
    }

    public void Skip()
    {
        if (IsFinished || IsCurrentStageCompleted)
            return;

        _stageStatuses[_currentStageIndex] = StageStatus.Completed;
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