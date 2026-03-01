using GoTrexia.Core.ValueObjects;

namespace GoTrexia.Core.Engine;

public sealed class DistanceCalculator
{
    private const double EarthRadiusMeters = 6_371_000;

    public double Calculate(GeoPoint pointA, GeoPoint pointB)
    {
        ArgumentNullException.ThrowIfNull(pointA);
        ArgumentNullException.ThrowIfNull(pointB);

        return CalculateMeters(
            new GeoCoordinate(pointA.Latitude, pointA.Longitude),
            new GeoCoordinate(pointB.Latitude, pointB.Longitude));
    }

    public double CalculateMeters(GeoCoordinate pointA, GeoCoordinate pointB)
    {
        ArgumentNullException.ThrowIfNull(pointA);
        ArgumentNullException.ThrowIfNull(pointB);

        var latitudeDelta = ToRadians(pointB.Latitude - pointA.Latitude);
        var longitudeDelta = ToRadians(pointB.Longitude - pointA.Longitude);

        var latitudeA = ToRadians(pointA.Latitude);
        var latitudeB = ToRadians(pointB.Latitude);

        var sinLatitude = Math.Sin(latitudeDelta / 2);
        var sinLongitude = Math.Sin(longitudeDelta / 2);

        var haversine =
            sinLatitude * sinLatitude +
            Math.Cos(latitudeA) * Math.Cos(latitudeB) * sinLongitude * sinLongitude;

        var centralAngle = 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));

        return EarthRadiusMeters * centralAngle;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180d;
    }
}
