using GoTrexia.Core;
using GoTrexia.Core.Engine;

public sealed class GameSession
{
    public GameEngine? Engine { get; private set; }
    public string? RootFolder { get; private set; }

    public void Start(GameDefinition definition,
                      StageCompletionEngine stageEngine,
                      string rootFolder)
    {
        Engine = new GameEngine(definition, stageEngine);
        RootFolder = rootFolder;
    }
}