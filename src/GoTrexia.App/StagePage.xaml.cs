using GoTrexia.Application;
using Microsoft.Extensions.DependencyInjection;

namespace GoTrexia;

public partial class StagePage : ContentPage
{
    private readonly GameSession _gameSession;
    private IDispatcherTimer? _locationTimer;

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
        StartLocationTracking();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///StartPage");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_locationTimer is not null)
        {
            _locationTimer.Stop();
            _locationTimer = null;
        }
    }

    private async void OnMapClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///StartPage/StagePage/MapPage");
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
            await Shell.Current.GoToAsync("///StartPage/EndPage");
            return;
        }

        LoadCurrentStage();
        _gameSession.StartCurrentStageTimer();
    }

    private async void OnCompleteClicked(object? sender, EventArgs e)
    {
        var engine = _gameSession.Engine!;
        if (engine.IsCurrentStageCompleted || !engine.CanCompleteCurrentStage)
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(AnswerPage));
    }

    private void LoadCurrentStage()
    {
        var engine = _gameSession.Engine!;
        var stage = engine.CurrentStage;
        StageTitleLabel.Text = stage.Name;
        StageLongDescriptionLabel.Text = stage.Description;
        var availableScore = engine.IsHintUsedForCurrentStage
            ? stage.Score / 2
            : stage.Score;
        StageScoreLabel.Text = $"Available score: {availableScore}";
        TotalScoreLabel.Text = $"Total score: {engine.TotalScore}";
        BackgroundImage.Source = BuildImagePath(_gameSession.RootFolder, stage.BackgroundImage);
        BackButtonImage.Source = BuildImagePath(_gameSession.RootFolder, engine.Settings.BackButton);

        UpdateActionButtons();
    }

    private void UpdateActionButtons()
    {
        var engine = _gameSession.Engine!;
        var isCompleted = engine.IsCurrentStageCompleted;

        SkipButton.IsEnabled = !isCompleted;
        CompleteButton.IsEnabled = !isCompleted && engine.CanCompleteCurrentStage;

        SkipButton.Opacity = SkipButton.IsEnabled ? 1 : 0.6;
        CompleteButton.Opacity = CompleteButton.IsEnabled ? 1 : 0.6;
    }

    private void StartLocationTracking()
    {
        _ = UpdateCompletionStateAsync();

        _locationTimer ??= Dispatcher.CreateTimer();
        _locationTimer.Interval = TimeSpan.FromSeconds(2);
        _locationTimer.Tick += async (_, _) => await UpdateCompletionStateAsync();
        _locationTimer.Start();
    }

    private async Task UpdateCompletionStateAsync()
    {
        var engine = _gameSession.Engine;
        if (engine is null || engine.IsFinished || engine.IsCurrentStageCompleted)
        {
            UpdateActionButtons();
            return;
        }

        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status != PermissionStatus.Granted)
        {
            UpdateActionButtons();
            return;
        }

        var lastKnownLocation = await Geolocation.GetLastKnownLocationAsync();
        if (lastKnownLocation is not null)
        {
            engine.UpdatePlayerPosition(new GoTrexia.Core.ValueObjects.GeoPoint(
                lastKnownLocation.Latitude,
                lastKnownLocation.Longitude));
        }

        UpdateActionButtons();
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
