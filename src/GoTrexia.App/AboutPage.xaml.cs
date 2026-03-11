using GoTrexia.Application;
using Microsoft.Extensions.DependencyInjection;

namespace GoTrexia;

public partial class AboutPage : ContentPage
{
    private readonly GameSession _gameSession;

    public AboutPage()
    {
        InitializeComponent();

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Service provider is not available.");

        _gameSession = services.GetRequiredService<GameSession>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var engine = _gameSession.Engine;
        if (engine is null)
        {
            return;
        }

        var totalGameScore = engine.Stages.Sum(x => x.Score);

        TitleLabel.Text = "About";
        TotalGameScoreLabel.Text = $"Total game score: {totalGameScore}";
        DescriptionLabel.Text = engine.StartScreen.Description;
        AuthorLabel.Text = engine.StartScreen.Author;

        BackgroundImage.Source = BuildImagePath(_gameSession.RootFolder, engine.StartScreen.BackgroundImage);
        BackButtonImage.Source = BuildImagePath(_gameSession.RootFolder, engine.Settings.BackButton);
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///StartPage");
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
