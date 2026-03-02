using GoTrexia.Application;
using GoTrexia.Core.Engine;
using GoTrexia.Infrastructure.Game;

namespace GoTrexia;

public partial class StartPage : ContentPage
{
    private GameSession? _gameSession;
    private StageCompletionEngine? _stageEngine;

    public StartPage()
    {
        InitializeComponent();
    }

    private async void OnLoadGameClicked(object? sender, EventArgs e)
    {
        EnsureServices();

        await using var stream =
        await FileSystem.OpenAppPackageFileAsync("sample-game.json");

        var loader = new GameDefinitionLoader();

        var definition = await loader.LoadAsync(stream);

        _gameSession.Start(definition, _stageEngine);

        await Shell.Current.GoToAsync(nameof(StagePage));
    }

    private void EnsureServices()
    {
        if (_gameSession is not null && _stageEngine is not null)
        {
            return;
        }

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Service provider is not available.");

        _gameSession = services.GetRequiredService<GameSession>();
        _stageEngine = services.GetRequiredService<StageCompletionEngine>();
    }
}
