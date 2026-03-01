namespace GoTrexia.Models
{
    public sealed record EndScreen
    {
        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string BackgroundImage { get; init; } = string.Empty;
    }
}
