using FluentAssertions;
using GoTrexia.Core.Engine;

namespace GoTrexia.Tests.Engine;

public class ScoreEngineTests
{
    private readonly ScoreEngine _sut = new();

    [Theory]
    [InlineData(100, 100)]
    [InlineData(0, 0)]
    public void Should_Return_FullPoints_When_NoHint_And_NotSkipped(int basePoints, int expectedPoints)
    {
        var result = _sut.Calculate(basePoints, hintUsed: false, skipped: false);

        result.EarnedPoints.Should().Be(expectedPoints);
        result.WasHintUsed.Should().BeFalse();
        result.WasSkipped.Should().BeFalse();
    }

    [Fact]
    public void Should_Return_HalfPoints_When_HintUsed()
    {
        var result = _sut.Calculate(basePoints: 100, hintUsed: true, skipped: false);

        result.EarnedPoints.Should().Be(50);
        result.WasHintUsed.Should().BeTrue();
        result.WasSkipped.Should().BeFalse();
    }

    [Fact]
    public void Should_Return_Zero_When_Skipped()
    {
        var result = _sut.Calculate(basePoints: 100, hintUsed: false, skipped: true);

        result.EarnedPoints.Should().Be(0);
        result.WasHintUsed.Should().BeFalse();
        result.WasSkipped.Should().BeTrue();
    }

    [Fact]
    public void Hint_Should_Not_Matter_When_Skipped()
    {
        var withHint = _sut.Calculate(basePoints: 100, hintUsed: true, skipped: true);
        var withoutHint = _sut.Calculate(basePoints: 100, hintUsed: false, skipped: true);

        withHint.EarnedPoints.Should().Be(0);
        withoutHint.EarnedPoints.Should().Be(0);

        withHint.WasSkipped.Should().BeTrue();
        withoutHint.WasSkipped.Should().BeTrue();
    }
}
