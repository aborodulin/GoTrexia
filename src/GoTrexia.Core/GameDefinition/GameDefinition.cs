namespace GoTrexia.Core;

public sealed record GameDefinition(
    GameSettings Settings,
    ScreenDefinition StartScreen,
    ScreenDefinition EndScreen,
    IReadOnlyList<StageDefinition> Stages
);
