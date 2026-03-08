using FluentAssertions;
using GoTrexia.Core;
using GoTrexia.Core.Engine;
using GoTrexia.Core.Stages;
using GoTrexia.Core.ValueObjects;

namespace GoTrexia.Tests.GameEngine;

public class GameEngineTests
{
    [Fact]
    public void Should_Complete_Single_Stage_And_Finish_Game_With_Full_Score()
    {
        var stage = new StageDefinition(
            Name: "Stage 1",
            Description: "Test stage",
            BackgroundImage: "stage1.jpg",
            SearchLocation: new GeoPoint(49.123, -122.456),
            HintLocation: new GeoPoint(49.123, -122.456),
            TargetLocation: new GeoPoint(49.123, -122.456),
            Score: 100,
            Answer: "test-answer");

        var definition = new GameDefinition(
            Settings: new GameSettings(10, 120, 50, 30, "back.png"),
            StartScreen: new ScreenDefinition("Start", "Start desc", "start.jpg", "Author"),
            EndScreen: new ScreenDefinition("End", "End desc", "end.jpg", ""),
            Stages: [stage],
            Answers: ["test-answer", "wrong-1", "wrong-2"]);

        var stageCompletionEngine = new StageCompletionEngine(new DistanceCalculator());
        var sut = new global::GameEngine(definition, stageCompletionEngine);

        var state = sut.UpdatePlayerPosition(new GeoPoint(49.123, -122.456));

        state.Status.Should().Be(StageStatus.AwaitingConfirmation);

        sut.ConfirmFound();

        sut.TotalScore.Should().Be(stage.Score);
        sut.IsFinished.Should().BeTrue();
    }
}
