using GoTrexia.Application;
using Microsoft.Extensions.DependencyInjection;

namespace GoTrexia;

public partial class StagePage : ContentPage
{
    private readonly GameSession _gameSession;

    public StagePage()
    {
        InitializeComponent();

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Service provider is not available.");

        _gameSession = services.GetRequiredService<GameSession>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadCurrentStage();
        _gameSession.StartCurrentStageTimer();
    }

    private async void OnMapClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MapPage));
    }

    private async void OnSkipClicked(object? sender, EventArgs e)
    {
        var engine = _gameSession.Engine!;
        if (engine.IsCurrentStageCompleted)
        {
            return;
        }

        engine.Skip();

        if (engine.IsFinished)
        {
            await Shell.Current.GoToAsync(nameof(EndPage));
            return;
        }

        LoadCurrentStage();
        _gameSession.StartCurrentStageTimer();
    }

    private async void OnCompleteClicked(object? sender, EventArgs e)
    {
        var engine = _gameSession.Engine!;
        if (engine.IsCurrentStageCompleted)
        {
            return;
        }

        engine.CompleteCurrentStage();

        if (engine.IsFinished)
        {
            await Shell.Current.GoToAsync(nameof(EndPage));
            return;
        }

        LoadCurrentStage();
        _gameSession.StartCurrentStageTimer();
    }

    private void LoadCurrentStage()
    {
        var engine = _gameSession.Engine!;
        var stage = engine.CurrentStage;
        StageTitleLabel.Text = stage.Name;
        StageLongDescriptionLabel.Text = stage.Description;
        StageScoreLabel.Text = $"Score: {stage.Score}";
        BackgroundImage.Source = BuildImagePath(_gameSession.RootFolder, stage.BackgroundImage);

        var isCompleted = engine.IsCurrentStageCompleted;
        SkipButton.IsEnabled = !isCompleted;
        CompleteButton.IsEnabled = !isCompleted;

        if (isCompleted)
        {
            SkipButton.Opacity = 0.6;
            CompleteButton.Opacity = 0.6;
        }
        else
        {
            SkipButton.Opacity = 1;
            CompleteButton.Opacity = 1;
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
