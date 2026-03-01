namespace GoTrexia.Models
{
    public sealed record GameInfo
    {
        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;
    }
}
