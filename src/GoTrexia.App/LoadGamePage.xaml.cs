using GoTrexia.Application;
using GoTrexia.Core.Engine;
using GoTrexia.Infrastructure.Game;
using Microsoft.Extensions.DependencyInjection;

namespace GoTrexia;

public partial class LoadGamePage : ContentPage
{
    private readonly GameSession _gameSession;
    private readonly StageCompletionEngine _stageEngine;

    public LoadGamePage()
    {
        InitializeComponent();

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Service provider is not available.");

        _gameSession = services.GetRequiredService<GameSession>();
        _stageEngine = services.GetRequiredService<StageCompletionEngine>();
    }

    private async void OnLoadGameClicked(object? sender, EventArgs e)
    {
        var zipLoader = new ZipGameLoader();
        var definitionLoader = new GameDefinitionLoader();

        string rootFolder;

        try
        {
            rootFolder = await zipLoader.ExtractAsync("SampleGame.zip");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            return;
        }

        var gameJsonPath = Path.Combine(rootFolder, "game.json");

        if (!File.Exists(gameJsonPath))
        {
            await DisplayAlert("Error", "game.json was not found in the loaded zip.", "OK");
            return;
        }

        await using var stream = File.OpenRead(gameJsonPath);
        var definition = await definitionLoader.LoadAsync(stream);

        _gameSession.Start(definition, _stageEngine, rootFolder);

        await Shell.Current.GoToAsync(nameof(StartPage));
    }
}
