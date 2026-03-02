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

        var stage = _gameSession.Engine!.CurrentStage;
        StageTitleLabel.Text = stage.Name;
    }

    private async void OnSkipClicked(object? sender, EventArgs e)
    {
        _gameSession.Engine!.Skip();

        if (_gameSession.Engine.IsFinished)
            await Shell.Current.GoToAsync(nameof(EndPage));
        else
            await DisplayAlert("Next", "Next stage", "OK");
    }
}
