using GoTrexia.Application;
using Microsoft.Maui.Controls.Maps;
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
        _ = EnableLocationAsync();
    }

    private async void OnSkipClicked(object? sender, EventArgs e)
    {
        _gameSession.Engine!.Skip();

        if (_gameSession.Engine.IsFinished)
        {
            await Shell.Current.GoToAsync(nameof(EndPage));
            return;
        }

        LoadCurrentStage();
    }

    private void LoadCurrentStage()
    {
        var engine = _gameSession.Engine!;
        var stage = engine.CurrentStage;
        StageTitleLabel.Text = stage.Name;
        StageDescriptionLabel.Text = stage.Description;
        StageScoreLabel.Text = $"Score: {stage.Score}";
        BackgroundImage.Source = BuildImagePath(_gameSession.RootFolder, stage.BackgroundImage);

        var center = new Location(stage.TargetLocation.Latitude, stage.TargetLocation.Longitude);
        var maxSearchRadius = engine.Settings.MaxSearchRadiusMeters;

        StageMap.MapElements.Clear();
        StageMap.MapElements.Add(new Circle
        {
            Center = center,
            Radius = Microsoft.Maui.Maps.Distance.FromMeters(maxSearchRadius),
            StrokeColor = Color.FromRgb(204, 153, 0),
            StrokeWidth = 4,
            FillColor = Color.FromRgba(255, 255, 153, 128)
        });

        StageMap.MoveToRegion(Microsoft.Maui.Maps.MapSpan.FromCenterAndRadius(
            center,
            Microsoft.Maui.Maps.Distance.FromMeters(maxSearchRadius * 2)));
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
                MoveCamera(lastKnownLocation);
            }
        }
    }

    private void MoveCamera(Location location)
    {
        var cameraRegion = Microsoft.Maui.Maps.MapSpan.FromCenterAndRadius(
            new Location(location.Latitude, location.Longitude),
            Microsoft.Maui.Maps.Distance.FromMeters(200));

        StageMap.MoveToRegion(cameraRegion);
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
