using GoTrexia.Application;
using GoTrexia.Core;
using Microsoft.Extensions.DependencyInjection;

namespace GoTrexia;

public partial class AnswerPage : ContentPage
{
    private readonly GameSession _gameSession;
    private readonly CompletedSoundPlayer _completedSoundPlayer;
    private int _availableScore;
    private int _wrongAnswersCount;
    private string _correctAnswer = string.Empty;

    public AnswerPage()
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

        var engine = _gameSession.Engine;
        if (engine is null || engine.IsFinished)
        {
            return;
        }

        var stage = engine.CurrentStage;
        _correctAnswer = stage.Answer;
        _wrongAnswersCount = 0;
        _availableScore = stage.Score;

        if (engine.IsHintUsedForCurrentStage)
        {
            _availableScore /= 2;
        }

        TitleLabel.Text = stage.Name;
        TotalScoreLabel.Text = $"Total score: {engine.TotalScore}";
        BackgroundImage.Source = BuildImagePath(_gameSession.RootFolder, stage.BackgroundImage);
        BackButtonImage.Source = BuildImagePath(_gameSession.RootFolder, engine.Settings.BackButton);
        UpdateAvailableScoreLabel();

        var allAnswers = engine.Answers.ToList();
        if (!allAnswers.Contains(_correctAnswer))
        {
            allAnswers.Add(_correctAnswer);
        }

        var options = AnswerGenerator.GenerateOptions(_correctAnswer, allAnswers);
        var buttons = GetAnswerButtons();

        for (var i = 0; i < buttons.Count; i++)
        {
            buttons[i].Text = options[i];
            buttons[i].IsEnabled = true;
            buttons[i].BackgroundColor = null;
            buttons[i].TextColor = null;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    private async void OnAnswerClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || string.IsNullOrWhiteSpace(button.Text))
        {
            return;
        }

        var engine = _gameSession.Engine;
        if (engine is null || engine.IsFinished)
        {
            return;
        }

        if (button.Text == _correctAnswer)
        {
            DisableAllAnswerButtons();
            button.BackgroundColor = Colors.Green;
            button.TextColor = Colors.White;

            _completedSoundPlayer.Play(_gameSession.RootFolder, engine.Settings.CompletedSound);

            await Task.Delay(TimeSpan.FromSeconds(2));

            engine.CompleteCurrentStage(_availableScore);

            if (engine.IsFinished)
            {
                await Shell.Current.GoToAsync("///StartPage/EndPage");
                return;
            }

            _gameSession.StartCurrentStageTimer();
            await Shell.Current.GoToAsync("///StartPage/StagePage");
            return;
        }

        button.IsEnabled = false;
        button.BackgroundColor = Colors.Gray;
        button.TextColor = Colors.White;

        _wrongAnswersCount++;

        if (_wrongAnswersCount == 1)
        {
            _availableScore /= 2;
        }
        else if (_wrongAnswersCount >= 2)
        {
            _availableScore = 0;
        }

        UpdateAvailableScoreLabel();
    }

    private void UpdateAvailableScoreLabel()
    {
        AvailableScoreLabel.Text = $"Available score: {_availableScore}";
    }

    private void DisableAllAnswerButtons()
    {
        foreach (var button in GetAnswerButtons())
        {
            button.IsEnabled = false;
        }
    }

    private IReadOnlyList<Button> GetAnswerButtons()
    {
        return [AnswerButton1, AnswerButton2, AnswerButton3];
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
