using GoTrexia.Core;
using GoTrexia.Core.Engine;

namespace GoTrexia.Application;

public sealed class GameSession
{
    public GameEngine? Engine { get; private set; }

    public void Start(GameDefinition definition,
                      StageCompletionEngine stageEngine)
    {
        Engine = new GameEngine(definition, stageEngine);
    }
}
