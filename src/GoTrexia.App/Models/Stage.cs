namespace GoTrexia.Models
{
    public sealed record Stage
    {
        public string Id { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string BackgroundImage { get; init; } = string.Empty;

        public GeoLocation FinalLocation { get; init; } = new();

        public int HintDelaySeconds { get; init; }

        public double HintRadiusMeters { get; init; }

        public int HintPrecisionDelaySeconds { get; init; }

        public int Points { get; init; }
    }
}
