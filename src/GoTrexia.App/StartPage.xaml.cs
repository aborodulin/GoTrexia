using GoTrexia.Application;
using GoTrexia.Core.Engine;
using GoTrexia.Infrastructure.Game;
using Microsoft.Extensions.DependencyInjection;

namespace GoTrexia;

public partial class StartPage : ContentPage
{
    private GameSession? _gameSession;
    private StageCompletionEngine? _stageEngine;
    private bool _isLoading;

    public StartPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        EnsureServices();

        await EnsureGameLoadedAsync();

        var engine = _gameSession!.Engine;
        if (engine is null)
        {
            return;
        }

        var gameScore = engine.Stages.Sum(stage => stage.Score);
        TotalScoreLabel.Text = $"Game: {gameScore}, Total: {engine.TotalScore}";
        BackgroundImage.Source = BuildImagePath(_gameSession.RootFolder, engine.StartScreen.BackgroundImage);
        StagesCollectionView.ItemsSource = engine.StageCheckpoints;

        StartButton.Text = engine.IsFinished
            ? "Completed"
            : _gameSession.HasGameStarted
                ? "Continue"
                : "Start";
    }

    private async void OnStartClicked(object? sender, EventArgs e)
    {
        var engine = _gameSession?.Engine;

        if (engine is null)
        {
            return;
        }

        if (engine.IsFinished)
        {
            await Shell.Current.GoToAsync(nameof(EndPage));
            return;
        }

        _gameSession!.StartCurrentStageTimer();

        await Shell.Current.GoToAsync("///StartPage/StagePage");
    }

    private async void OnAboutClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AboutPage));
    }

    private void EnsureServices()
    {
        if (_gameSession is not null)
        {
            return;
        }

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Service provider is not available.");

        _gameSession = services.GetRequiredService<GameSession>();
        _stageEngine = services.GetRequiredService<StageCompletionEngine>();
    }

    private async Task EnsureGameLoadedAsync()
    {
        if (_gameSession?.Engine is not null || _isLoading)
        {
            return;
        }

        _isLoading = true;

        try
        {
            var zipLoader = new ZipGameLoader();
            var definitionLoader = new GameDefinitionLoader();

            var rootFolder = await zipLoader.ExtractAsync("SampleGame.zip");
            var gameJsonPath = Path.Combine(rootFolder, "game.json");

            if (!File.Exists(gameJsonPath))
            {
                await DisplayAlert("Error", "game.json was not found in the loaded zip.", "OK");
                return;
            }

            await using var stream = File.OpenRead(gameJsonPath);
            var definition = await definitionLoader.LoadAsync(stream);

            _gameSession!.Start(definition, _stageEngine!, rootFolder);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Load error", ex.Message, "OK");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static string BuildImagePath(string? rootFolder, string fileName)
    {
        if (string.IsNullOrWhiteSpace(rootFolder))
        {
            return fileName;
        }

        return Path.Combine(rootFolder, fileName);
    }
}
