using GoTrexia.Application;
using Microsoft.Extensions.DependencyInjection;

namespace GoTrexia;

public partial class StartPage : ContentPage
{
    private GameSession? _gameSession;

    public StartPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        EnsureServices();

        var engine = _gameSession!.Engine;
        if (engine is null)
        {
            return;
        }

        TitleLabel.Text = engine.StartScreen.Title;
        DescriptionLabel.Text = engine.StartScreen.Description;
        AuthorLabel.Text = engine.StartScreen.Author;
        BackgroundImage.Source = BuildImagePath(_gameSession.RootFolder, engine.StartScreen.BackgroundImage);
        StagesCollectionView.ItemsSource = engine.StageCheckpoints;
    }

    private async void OnStartClicked(object? sender, EventArgs e)
    {
        var engine = _gameSession?.Engine;

        if (engine is null || engine.IsFinished)
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(StagePage));
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
