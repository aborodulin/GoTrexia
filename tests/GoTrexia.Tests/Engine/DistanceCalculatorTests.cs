using FluentAssertions;
using GoTrexia.Core.Engine;
using GoTrexia.Core.ValueObjects;

namespace GoTrexia.Tests.Engine;

public class DistanceCalculatorTests
{
    private readonly DistanceCalculator _sut = new();

    [Fact]
    public void Distance_Between_Same_Points_Should_Be_0()
    {
        var point = new GeoCoordinate(48.8566, 2.3522);

        var result = _sut.CalculateMeters(point, point);

        result.Should().Be(0d);
    }

    [Fact]
    public void Distance_Between_Two_Known_Coordinates_Should_Be_Approximately_Correct()
    {
        var pointA = new GeoCoordinate(0, 0);
        var pointB = new GeoCoordinate(1, 0);

        var result = _sut.CalculateMeters(pointA, pointB);

        result.Should().BeApproximately(111_194.9266d, 5d);
    }

    [Fact]
    public void Constructor_Of_GeoCoordinate_Should_Throw_For_Invalid_Latitude()
    {
        Action act = () => _ = new GeoCoordinate(90.0001, 0);

        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("latitude");
    }

    [Fact]
    public void Constructor_Should_Throw_For_Invalid_Longitude()
    {
        Action act = () => _ = new GeoCoordinate(0, 180.0001);

        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("longitude");
    }
}
