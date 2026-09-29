using System.Diagnostics;

namespace SilksongOverlay.Core;

public sealed class TimerService
{
    private readonly Stopwatch _stopwatch = new();
    private TimeSpan _offset = TimeSpan.Zero;

    public bool IsRunning => _stopwatch.IsRunning;
    public TimeSpan Elapsed => _offset + _stopwatch.Elapsed;

    public string GetFormattedText()
    {
        var elapsed = Elapsed;
        return $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }

    public void RestoreElapsed(TimeSpan elapsed)
    {
        _stopwatch.Reset();
        _offset = elapsed;
    }

    public void Start()
    {
        if (_stopwatch.IsRunning)
        {
            return;
        }

        _stopwatch.Start();
    }

    public void Stop()
    {
        if (!_stopwatch.IsRunning)
        {
            return;
        }

        _stopwatch.Stop();
    }

    public void Reset()
    {
        _stopwatch.Reset();
        _offset = TimeSpan.Zero;
    }
}
