using System.Diagnostics;

namespace SilksongOverlay.Core;

public sealed class TimerService
{
    private readonly Stopwatch _stopwatch = new();

    public bool IsRunning => _stopwatch.IsRunning;

    public string GetFormattedText()
    {
        var elapsed = _stopwatch.Elapsed;
        return $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
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
    }
}
