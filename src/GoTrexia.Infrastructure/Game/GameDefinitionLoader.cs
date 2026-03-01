using GoTrexia.Core;
using System.Text.Json;

public sealed class GameDefinitionLoader
{
    public async Task<GameDefinition> LoadAsync(string filePath)
    {
        await using var stream = File.OpenRead(filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var definition = await JsonSerializer.DeserializeAsync<GameDefinition>(
            stream,
            options);

        if (definition is null)
            throw new InvalidOperationException("Invalid game definition file.");

        return definition;
    }
}