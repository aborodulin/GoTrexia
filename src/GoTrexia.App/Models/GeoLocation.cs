namespace GoTrexia.Models
{
    public sealed record GeoLocation
    {
        public double Latitude { get; init; }

        public double Longitude { get; init; }
    }
}
