using GoTrexia.Application;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Extensions.DependencyInjection;

namespace GoTrexia;

public partial class MapPage : ContentPage
{
    private readonly GameSession _gameSession;
    private IDispatcherTimer? _timer;

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

    private void OnHintClicked(object? sender, EventArgs e)
    {
        var engine = _gameSession.Engine!;
        if (engine.IsCurrentStageCompleted || engine.IsHintUsedForCurrentStage)
        {
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

        StageTitleLabel.Text = stage.Name;
        BackgroundImage.Source = BuildImagePath(_gameSession.RootFolder, stage.BackgroundImage);

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
        _timer.Tick += (_, _) => UpdateCountdown();
        _timer.Start();
    }

    private void UpdateCountdown()
    {
        var remainingSeconds = _gameSession.GetRemainingHintSeconds();

        if (remainingSeconds > 0)
        {
            var minutes = remainingSeconds / 60;
            var seconds = remainingSeconds % 60;
            CountdownLabel.Text = $"{minutes:D2}:{seconds:D2}";
            CountdownLabel.IsVisible = true;
            HintButton.IsVisible = false;
            return;
        }

        CountdownLabel.IsVisible = false;
        HintButton.IsVisible = true;
        HintButton.IsEnabled = !_gameSession.Engine!.IsHintUsedForCurrentStage;
        HintButton.Text = _gameSession.Engine.IsHintUsedForCurrentStage ? "Hint used" : "Hint";
    }

    private async Task EnableLocationAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status == PermissionStatus.Granted)
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

    private static string BuildImagePath(string? rootFolder, string fileName)
    {
        if (string.IsNullOrWhiteSpace(rootFolder))
        {
            return fileName;
        }

        return Path.Combine(rootFolder, fileName);
    }
}
