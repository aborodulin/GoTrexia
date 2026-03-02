using GoTrexia.Core;
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

        var definition =
            await JsonSerializer.DeserializeAsync<GameDefinition>(
                stream,
                options);

        if (definition is null)
            throw new InvalidOperationException("Invalid game definition file.");

        return definition;
    }
      

    private sealed record GameDefinitionPayload(
        GameSettingsPayload Settings,
        ScreenPayload StartScreen,
        ScreenPayload EndScreen,
        IReadOnlyList<StagePayload> Stages);

    private sealed record GameSettingsPayload(
        double MinConfirmationRadiusMeters,
        double MaxSearchRadiusMeters,
        double HintRadiusMeters,
        int HintButtonTimeoutSeconds);

    private sealed record ScreenPayload(
        string Title,
        string Description,
        [property: JsonPropertyName("backgroundImage")] string BackgroundImage,
        string Author);

    private sealed record StagePayload(
        string Name,
        string Description,
        [property: JsonPropertyName("backgroundImage")] string BackgroundImage,
        TargetLocationPayload TargetLocation,
        int Score);

    private sealed record TargetLocationPayload(
        double Latitude,
        double Longitude);
}