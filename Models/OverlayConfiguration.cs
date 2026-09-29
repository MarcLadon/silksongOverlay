namespace SilksongOverlay.Models;

public sealed class OverlayConfiguration
{
    public List<Objective> Objectives { get; set; } = new();

    public int CurrentObjectiveIndex { get; set; }

    public int DeathCount { get; set; }

    public double TimerElapsedSeconds { get; set; }

    public bool TimerRunning { get; set; }

    public List<WidgetConfiguration> Widgets { get; set; } = new();
}
