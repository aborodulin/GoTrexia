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
    private int _mapModeIndex;

    private static readonly IReadOnlyList<(string Label, Microsoft.Maui.Maps.MapType Type)> MapModes =
    [
        ("Map", Microsoft.Maui.Maps.MapType.Street),
        ("Satellite", Microsoft.Maui.Maps.MapType.Satellite),
        ("Hybrid", Microsoft.Maui.Maps.MapType.Hybrid)
    ];

    public MapPage()
    {
        InitializeComponent();

        ApplyMapMode(0);

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

    private void OnMapModeButtonClicked(object? sender, EventArgs e)
    {
        var nextIndex = (_mapModeIndex + 1) % MapModes.Count;
        ApplyMapMode(nextIndex);
    }

    private void ApplyMapMode(int index)
    {
        _mapModeIndex = index;
        var mapMode = MapModes[_mapModeIndex];
        StageMap.MapType = mapMode.Type;
        MapModeButton.Text = mapMode.Label;
    }

    private void LoadCurrentStageMap()
    {
        var engine = _gameSession.Engine!;
        var stage = engine.CurrentStage;
        var availableScore = engine.IsHintUsedForCurrentStage
            ? stage.Score / 2
            : stage.Score;

        StageTitleLabel.Text = stage.Name;
        TotalScoreLabel.Text = $"Stage: {availableScore}, Total: {engine.TotalScore}";
        HintLabel.Text = "Go to search area";
        HintLabel.IsVisible = true;
        BackgroundImage.Source = BuildImagePath(_gameSession.RootFolder, stage.BackgroundImage);
        BackButtonImage.Source = BuildImagePath(_gameSession.RootFolder, engine.Settings.BackButton);

        var isHintUsed = engine.IsHintUsedForCurrentStage;
        var radiusMeters = isHintUsed
            ? stage.HintLocation.RadiusMeters
            : stage.SearchLocation.RadiusMeters;

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
        var canComplete = engine.CanCompleteCurrentStage;
        var isHintUsed = engine.IsHintUsedForCurrentStage;

        CompleteButton.IsEnabled = canComplete;

        if (canComplete)
        {
            CountdownLabel.IsVisible = false;
            HintButton.IsVisible = false;
            CompleteButton.IsVisible = true;
            HintLabel.IsVisible = true;
            HintLabel.Text = "Have you found? Press Complete!";
            return;
        }

        if (isHintUsed)
        {
            CountdownLabel.IsVisible = false;
            CompleteButton.IsVisible = false;
            HintLabel.IsVisible = true;
            HintLabel.Text = "Skip stage if it's too hard";
            HintButton.IsVisible = true;
            HintButton.IsEnabled = true;
            HintButton.Text = "Skip";
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
            HintLabel.IsVisible = true;
            HintLabel.Text = "Go to search area";
            return;
        }

        CountdownLabel.IsVisible = false;
        CompleteButton.IsVisible = false;
        HintLabel.IsVisible = true;
        HintLabel.Text = "Using hint will reduce score";
        HintButton.IsVisible = true;
        HintButton.IsEnabled = true;
        HintButton.Text = "Hint";
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
