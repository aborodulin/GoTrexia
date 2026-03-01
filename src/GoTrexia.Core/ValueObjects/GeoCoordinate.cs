namespace GoTrexia.Core.ValueObjects;

public sealed record GeoCoordinate
{
    public GeoCoordinate(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        }

        if (longitude < -180 || longitude > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; init; }

    public double Longitude { get; init; }
}
