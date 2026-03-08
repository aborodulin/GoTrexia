using GoTrexia.Application;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Extensions.DependencyInjection;

namespace GoTrexia;

public partial class MapPage : ContentPage
{
    private readonly GameSession _gameSession;
    private IDispatcherTimer? _timer;
    private bool _hasLocationPermission;
    private bool _permissionInitialized;

    public MapPage()
    {
        InitializeComponent();

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Service provider is not available.");

        _gameSession = services.GetRequiredService<GameSession>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadCurrentStageMap();
        StartCountdown();
        _ = EnableLocationAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_timer is not null)
        {
            _timer.Stop();
            _timer = null;
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///StartPage/StagePage");
    }

    private async void OnHintClicked(object? sender, EventArgs e)
    {
        var engine = _gameSession.Engine!;
        if (engine.IsCurrentStageCompleted)
        {
            return;
        }

        if (engine.IsHintUsedForCurrentStage)
        {
            engine.Skip();

            if (engine.IsFinished)
            {
                await Shell.Current.GoToAsync("///StartPage/EndPage");
                return;
            }

            _gameSession.StartCurrentStageTimer();
            await Shell.Current.GoToAsync("///StartPage/StagePage");
            return;
        }

        engine.UseHint();
        LoadCurrentStageMap();
        UpdateCountdown();
    }

    private void LoadCurrentStageMap()
    {
        var engine = _gameSession.Engine!;
        var stage = engine.CurrentStage;
        var availableScore = engine.IsHintUsedForCurrentStage
            ? stage.Score / 2
            : stage.Score;

        StageTitleLabel.Text = stage.Name;
        AvailableScoreLabel.Text = $"Available score: {availableScore}";
        TotalScoreLabel.Text = $"Total score: {engine.TotalScore}";
        BackgroundImage.Source = BuildImagePath(_gameSession.RootFolder, stage.BackgroundImage);
        BackButtonImage.Source = BuildImagePath(_gameSession.RootFolder, engine.Settings.BackButton);

        var isHintUsed = engine.IsHintUsedForCurrentStage;
        var radiusMeters = isHintUsed
            ? engine.Settings.HintRadiusMeters
            : engine.Settings.MaxSearchRadiusMeters;

        var locationSource = isHintUsed ? stage.HintLocation : stage.SearchLocation;
        var center = new Location(locationSource.Latitude, locationSource.Longitude);

        StageMap.MapElements.Clear();
        StageMap.MapElements.Add(new Circle
        {
            Center = center,
            Radius = Microsoft.Maui.Maps.Distance.FromMeters(radiusMeters),
            StrokeColor = Color.FromRgb(204, 153, 0),
            StrokeWidth = 4,
            FillColor = Color.FromRgba(255, 255, 153, 128)
        });

        StageMap.MoveToRegion(Microsoft.Maui.Maps.MapSpan.FromCenterAndRadius(
            center,
            Microsoft.Maui.Maps.Distance.FromMeters(radiusMeters * 2)));
    }

    private void StartCountdown()
    {
        UpdateCountdown();

        _timer ??= Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += async (_, _) =>
        {
            await UpdateLocationStateAsync();
            UpdateCountdown();
        };
        _timer.Start();
    }

    private void UpdateCountdown()
    {
        var engine = _gameSession.Engine!;

        if (engine.CanCompleteCurrentStage)
        {
            CountdownLabel.IsVisible = false;
            HintButton.IsVisible = false;
            CompleteButton.IsVisible = true;
            return;
        }

        var remainingSeconds = _gameSession.GetRemainingHintSeconds();

        if (remainingSeconds > 0)
        {
            var minutes = remainingSeconds / 60;
            var seconds = remainingSeconds % 60;
            CountdownLabel.Text = $"{minutes:D2}:{seconds:D2}";
            CountdownLabel.IsVisible = true;
            HintButton.IsVisible = false;
            CompleteButton.IsVisible = false;
            return;
        }

        CountdownLabel.IsVisible = false;
        HintButton.IsVisible = true;
        CompleteButton.IsVisible = false;

        var isHintUsed = _gameSession.Engine!.IsHintUsedForCurrentStage;
        HintButton.IsEnabled = true;
        HintButton.Text = isHintUsed ? "Skip" : "Hint";
    }

    private async Task EnableLocationAsync()
    {
        await UpdateLocationStateAsync();
    }

    private async Task UpdateLocationStateAsync()
    {
        if (!_permissionInitialized)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            _hasLocationPermission = status == PermissionStatus.Granted;
            _permissionInitialized = true;
        }

        if (_hasLocationPermission)
        {
            StageMap.IsShowingUser = true;

            var lastKnownLocation = await Geolocation.GetLastKnownLocationAsync();
            if (lastKnownLocation is not null)
            {
                _gameSession.Engine!.UpdatePlayerPosition(new GoTrexia.Core.ValueObjects.GeoPoint(
                    lastKnownLocation.Latitude,
                    lastKnownLocation.Longitude));
            }
        }
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

    private static string BuildImagePath(string? rootFolder, string fileName)
    {
        if (string.IsNullOrWhiteSpace(rootFolder))
        {
            return fileName;
        }

        return Path.Combine(rootFolder, fileName);
    }
}
