namespace SilksongOverlay.Models;

public sealed class OverlayConfiguration
{
    public List<Objective> Objectives { get; set; } = new();

    public int CurrentObjectiveIndex { get; set; }

    public int DeathCount { get; set; }

    public string TimerText { get; set; } = "00:00:00";

    public bool TimerRunning { get; set; }

    public List<WidgetConfiguration> Widgets { get; set; } = new();
}
