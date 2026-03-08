using GoTrexia.Core;
using GoTrexia.Core.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GoTrexia.Infrastructure.Game;

public sealed class GameDefinitionLoader
{
    public async Task<GameDefinition> LoadAsync(Stream stream)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var payload =
            await JsonSerializer.DeserializeAsync<GameDefinitionPayload>(
                stream,
                options);

        if (payload is null)
            throw new InvalidOperationException("Invalid game definition file.");

        return new GameDefinition(
            new GameSettings(
                payload.Settings.MinConfirmationRadiusMeters,
                payload.Settings.MaxSearchRadiusMeters,
                payload.Settings.HintRadiusMeters,
                payload.Settings.HintButtonTimeoutSeconds,
                payload.Settings.BackButton),
            new ScreenDefinition(
                payload.StartScreen.Title,
                payload.StartScreen.Description,
                payload.StartScreen.BackgroundImage,
                payload.StartScreen.Author),
            new ScreenDefinition(
                payload.EndScreen.Title,
                payload.EndScreen.Description,
                payload.EndScreen.BackgroundImage,
                payload.EndScreen.Author),
            payload.Stages
                .Select(stage => new StageDefinition(
                    stage.Name,
                    stage.Description,
                    stage.BackgroundImage,
                    new GeoPoint(stage.SearchLocation.Latitude, stage.SearchLocation.Longitude),
                    new GeoPoint(stage.HintLocation.Latitude, stage.HintLocation.Longitude),
                    new GeoPoint(stage.TargetLocation.Latitude, stage.TargetLocation.Longitude),
                    stage.Score,
                    stage.Answer))
                .ToList(),
            payload.Answers);
    }
      

    private sealed record GameDefinitionPayload(
        GameSettingsPayload Settings,
        ScreenPayload StartScreen,
        ScreenPayload EndScreen,
        IReadOnlyList<StagePayload> Stages,
        IReadOnlyList<string> Answers);

    private sealed record GameSettingsPayload(
        double MinConfirmationRadiusMeters,
        double MaxSearchRadiusMeters,
        double HintRadiusMeters,
        int HintButtonTimeoutSeconds,
        string BackButton);

    private sealed record ScreenPayload(
        string Title,
        string Description,
        [property: JsonPropertyName("backgroundImage")] string BackgroundImage,
        string Author);

    private sealed record StagePayload(
        string Name,
        string Description,
        [property: JsonPropertyName("backgroundImage")] string BackgroundImage,
        TargetLocationPayload SearchLocation,
        TargetLocationPayload HintLocation,
        TargetLocationPayload TargetLocation,
        int Score,
        string Answer);

    private sealed record TargetLocationPayload(
        double Latitude,
        double Longitude);
}