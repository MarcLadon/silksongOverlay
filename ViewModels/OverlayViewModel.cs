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

    public string CurrentObjectiveName => ObjectiveService.CurrentObjective?.Name ?? "Terminé";
    public string CurrentObjectiveLocation => ObjectiveService.CurrentObjective?.Location ?? "—";
    public string? CurrentObjectiveImagePath => ObjectiveService.CurrentObjective?.ImagePath;
    public double CompletionPercentage => ObjectiveService.CompletionPercentage;

    public void CompleteCurrentObjective() => ObjectiveService.CompleteCurrentObjective();
    public void GoPreviousObjective() => ObjectiveService.GoToPreviousObjective();

