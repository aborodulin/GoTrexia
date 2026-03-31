using Android.Media;

namespace GoTrexia;

public sealed class CompletedSoundPlayer
{
    private MediaPlayer? _mediaPlayer;

    public void Play(string? rootFolder, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var filePath = string.IsNullOrWhiteSpace(rootFolder)
            ? fileName
            : Path.Combine(rootFolder, fileName);

        if (!File.Exists(filePath))
        {
            return;
        }

        try
        {
            StopCurrentPlayer();

            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.Completion += OnPlaybackCompleted;
            _mediaPlayer.SetDataSource(filePath);
            _mediaPlayer.Prepare();
            _mediaPlayer.Start();
        }
        catch
        {
            StopCurrentPlayer();
        }
    }

    private void OnPlaybackCompleted(object? sender, EventArgs e)
    {
        StopCurrentPlayer();
    }

    private void StopCurrentPlayer()
    {
        if (_mediaPlayer is null)
        {
            return;
        }

        _mediaPlayer.Completion -= OnPlaybackCompleted;

        if (_mediaPlayer.IsPlaying)
        {
            _mediaPlayer.Stop();
        }

        _mediaPlayer.Release();
        _mediaPlayer.Dispose();
        _mediaPlayer = null;
    }
}
