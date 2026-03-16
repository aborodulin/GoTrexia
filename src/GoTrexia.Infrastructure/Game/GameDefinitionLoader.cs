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

        if (payload.Settings is null)
            throw new InvalidOperationException("Game settings are required.");

        if (string.IsNullOrWhiteSpace(payload.Settings.BackButton))
            throw new InvalidOperationException("settings.backButton is required.");

        if (payload.Stages is null || payload.Stages.Count == 0)
            throw new InvalidOperationException("At least one stage is required.");

        var stages = payload.Stages
            .Select((stage, index) =>
            {
                ValidateStage(stage, index);

                return new StageDefinition(
                    stage.Name!,
                    stage.Description!,
                    stage.BackgroundImage!,
                    stage.HintButtonTimeoutSeconds!.Value,
                    CreateGeoPoint(stage.SearchLocation!, "searchLocation", index),
                    CreateGeoPoint(stage.HintLocation!, "hintLocation", index),
                    CreateGeoPoint(stage.TargetLocation!, "targetLocation", index),
                    stage.Score,
                    stage.Answer);
            })
            .ToList();

        return new GameDefinition(
            new GameSettings(
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
            stages,
            payload.Answers);
    }
      

    private sealed record GameDefinitionPayload(
        GameSettingsPayload Settings,
        ScreenPayload StartScreen,
        ScreenPayload EndScreen,
        IReadOnlyList<StagePayload> Stages,
        IReadOnlyList<string> Answers);

    private sealed record GameSettingsPayload(
        string? BackButton);

    private sealed record ScreenPayload(
        string Title,
        string Description,
        [property: JsonPropertyName("backgroundImage")] string BackgroundImage,
        string Author);

    private sealed record StagePayload(
        string? Name,
        string? Description,
        [property: JsonPropertyName("backgroundImage")] string BackgroundImage,
        int? HintButtonTimeoutSeconds,
        TargetLocationPayload? SearchLocation,
        TargetLocationPayload? HintLocation,
        TargetLocationPayload? TargetLocation,
        int Score,
        string Answer);

    private sealed record TargetLocationPayload(
        double? Radius,
        double? Latitude,
        double? Longitude);

    private static void ValidateStage(StagePayload stage, int stageIndex)
    {
        var stageName = $"stages[{stageIndex}]";

        if (string.IsNullOrWhiteSpace(stage.Name))
            throw new InvalidOperationException($"{stageName}.name is required.");

        if (string.IsNullOrWhiteSpace(stage.Description))
            throw new InvalidOperationException($"{stageName}.description is required.");

        if (string.IsNullOrWhiteSpace(stage.BackgroundImage))
            throw new InvalidOperationException($"{stageName}.backgroundImage is required.");

        if (!stage.HintButtonTimeoutSeconds.HasValue || stage.HintButtonTimeoutSeconds.Value <= 0)
            throw new InvalidOperationException($"{stageName}.hintButtonTimeoutSeconds must be greater than 0.");

        ValidateLocation(stage.TargetLocation, stageIndex, "targetLocation");
        ValidateLocation(stage.HintLocation, stageIndex, "hintLocation");
        ValidateLocation(stage.SearchLocation, stageIndex, "searchLocation");
    }

    private static GeoPoint CreateGeoPoint(TargetLocationPayload location, string locationName, int stageIndex)
    {
        var stageName = $"stages[{stageIndex}]";

        return new GeoPoint(
            location.Latitude!.Value,
            location.Longitude!.Value,
            location.Radius!.Value);
    }

    private static void ValidateLocation(TargetLocationPayload? location, int stageIndex, string locationName)
    {
        var stageName = $"stages[{stageIndex}]";

        if (location is null)
            throw new InvalidOperationException($"{stageName}.{locationName} is required.");

        if (!location.Radius.HasValue || location.Radius.Value <= 0)
            throw new InvalidOperationException($"{stageName}.{locationName}.radius must be greater than 0.");

        if (!location.Latitude.HasValue)
            throw new InvalidOperationException($"{stageName}.{locationName}.latitude is required.");

        if (!location.Longitude.HasValue)
            throw new InvalidOperationException($"{stageName}.{locationName}.longitude is required.");

        var latitude = location.Latitude.Value;
        var longitude = location.Longitude.Value;

        if (latitude < -90 || latitude > 90)
            throw new InvalidOperationException($"{stageName}.{locationName}.latitude must be between -90 and 90.");

        if (longitude < -180 || longitude > 180)
            throw new InvalidOperationException($"{stageName}.{locationName}.longitude must be between -180 and 180.");
    }
}