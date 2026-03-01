using FluentAssertions;
using GoTrexia.Core.Engine;
using GoTrexia.Core.Stages;
using GoTrexia.Core.ValueObjects;

namespace GoTrexia.Tests.Engine;

public class StageCompletionEngineTests
{
    private readonly StageCompletionEngine _sut = new(new DistanceCalculator());

    [Fact]
    public void If_Outside_Radius_Should_Be_InProgress()
    {
        var player = new GeoPoint(0.01, 0);
        var target = new GeoPoint(0, 0);
        var settings = new StageSettings(100);

        var result = _sut.UpdatePosition(player, target, settings);

        result.Status.Should().Be(StageStatus.InProgress);
        result.CanConfirm.Should().BeFalse();
    }

    [Fact]
    public void If_Inside_Radius_Should_Be_AwaitingConfirmation()
    {
        var player = new GeoPoint(0.0001, 0);
        var target = new GeoPoint(0, 0);
        var settings = new StageSettings(20);

        var result = _sut.UpdatePosition(player, target, settings);

        result.Status.Should().Be(StageStatus.AwaitingConfirmation);
        result.CanConfirm.Should().BeTrue();
    }

    [Fact]
    public void ConfirmFound_Should_Be_Completed()
    {
        var result = _sut.ConfirmFound();

        result.Status.Should().Be(StageStatus.Completed);
        result.DistanceToTargetMeters.Should().Be(0);
        result.CanConfirm.Should().BeFalse();
    }

    [Fact]
    public void Skip_Should_Be_Skipped()
    {
        var result = _sut.Skip();

        result.Status.Should().Be(StageStatus.Skipped);
        result.DistanceToTargetMeters.Should().Be(0);
        result.CanConfirm.Should().BeFalse();
    }
}
