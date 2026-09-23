using SilksongOverlay.Core;
using SilksongOverlay.Models;

namespace SilksongOverlay.ViewModels;

public sealed class OverlayViewModel
{
    public ObjectiveService ObjectiveService { get; }
    public TimerService TimerService { get; } = new();
    public int DeathCount { get; set; }

    public OverlayViewModel(List<Objective> objectives)
    {
        ObjectiveService = new ObjectiveService(objectives);
    }

    public string CurrentObjectiveName =>
        ObjectiveService.CurrentObjective?.Name ?? "Terminé";

    public string CurrentObjectiveLocation =>
        ObjectiveService.CurrentObjective?.Location ?? "—";

    public string? CurrentObjectiveImagePath =>
        ObjectiveService.CurrentObjective?.ImagePath;

    public double CompletionPercentage =>
        ObjectiveService.CompletionPercentage;

    public string TimerText =>
        TimerService.GetFormattedText();

    public string DeathText =>
        DeathCount.ToString();

    public void CompleteCurrentObjective()
    {
        ObjectiveService.CompleteCurrentObjective();
    }

    public void GoPreviousObjective()
    {
        ObjectiveService.GoToPreviousObjective();
    }

    public void AddDeath()
    {
        DeathCount++;
    }

    public void ToggleTimer()
    {
        if (TimerService.IsRunning)
        {
            TimerService.Stop();
        }
        else
        {
            TimerService.Start();
        }
    }

    public void ResetTimer()
    {
        TimerService.Reset();
    }
}
