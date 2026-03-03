public sealed record StageCheckpoint(
    string Name,
    string Description,
    int Score,
    bool IsChecked,
    bool IsSkipped
);
