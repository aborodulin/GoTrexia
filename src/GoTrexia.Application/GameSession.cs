using GoTrexia.Core;
using GoTrexia.Core.Engine;

public sealed class GameSession
{
    private readonly Dictionary<int, DateTimeOffset> _stageStartedAt = [];
    private readonly Dictionary<int, bool> _stageCompletionSoundState = [];

    public GameEngine? Engine { get; private set; }
    public string? RootFolder { get; private set; }

    public bool HasGameStarted => _stageStartedAt.Count > 0;

    public void Start(GameDefinition definition,
                      StageCompletionEngine stageEngine,
                      string rootFolder)
    {
        Engine = new GameEngine(definition, stageEngine);
        RootFolder = rootFolder;
        _stageStartedAt.Clear();
        _stageCompletionSoundState.Clear();
    }

    public void StartCurrentStageTimer()
    {
        if (Engine is null || Engine.IsFinished)
        {
            return;
        }

        var stageIndex = Engine.CurrentStageIndex;
        if (!_stageStartedAt.ContainsKey(stageIndex))
        {
            _stageStartedAt[stageIndex] = DateTimeOffset.UtcNow;
        }
    }

    public int GetRemainingHintSeconds()
    {
        if (Engine is null || Engine.IsFinished)
        {
            return 0;
        }

        var timeout = Engine.CurrentStage.HintButtonTimeoutSeconds;
        var stageIndex = Engine.CurrentStageIndex;

        if (!_stageStartedAt.TryGetValue(stageIndex, out var startedAt))
        {
            return timeout;
        }

        var elapsed = (int)(DateTimeOffset.UtcNow - startedAt).TotalSeconds;
        return Math.Max(0, timeout - elapsed);
    }

    public bool TryMarkCompletionSoundPlayed()
    {
        if (Engine is null || Engine.IsFinished)
        {
            return false;
        }

        var stageIndex = Engine.CurrentStageIndex;
        var canComplete = Engine.CanCompleteCurrentStage;
        _stageCompletionSoundState.TryGetValue(stageIndex, out var wasInTargetArea);

        _stageCompletionSoundState[stageIndex] = canComplete;

        return canComplete && !wasInTargetArea;
    }
}