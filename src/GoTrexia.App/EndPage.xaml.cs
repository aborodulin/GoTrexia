using GoTrexia.Application;
using Microsoft.Extensions.DependencyInjection;

namespace GoTrexia;

public partial class EndPage : ContentPage
{
    private readonly GameSession _gameSession;
    private readonly CompletedSoundPlayer _completedSoundPlayer;

    public EndPage()
    {
        InitializeComponent();

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Service provider is not available.");

        _gameSession = services.GetRequiredService<GameSession>();
        _completedSoundPlayer = services.GetRequiredService<CompletedSoundPlayer>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var engine = _gameSession.Engine!;
        TitleLabel.Text = engine.EndScreen.Title;
        DescriptionLabel.Text = engine.EndScreen.Description;
        AuthorLabel.Text = engine.EndScreen.Author;
        ScoreLabel.Text = $"Score: {engine.TotalScore}";
        BackgroundImage.Source = BuildImagePath(_gameSession.RootFolder, engine.EndScreen.BackgroundImage);
        BackButtonImage.Source = BuildImagePath(_gameSession.RootFolder, engine.Settings.BackButton);
        _completedSoundPlayer.Play(_gameSession.RootFolder, engine.Settings.CompletedSound);
    }

    private async void OnBackToStartClicked(object? sender, EventArgs e)
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
