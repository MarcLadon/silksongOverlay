using SilksongOverlay.Models;

namespace SilksongOverlay.Core;

public sealed class ObjectiveService
{
    private readonly List<Objective> _objectives;

    public ObjectiveService(List<Objective> objectives)
    {
        _objectives = objectives ?? new List<Objective>();
        RefreshOrder();
    }

    public IReadOnlyList<Objective> Objectives => _objectives;
    public int CurrentObjectiveIndex { get; private set; }

    public Objective? CurrentObjective =>
        CurrentObjectiveIndex >= 0 && CurrentObjectiveIndex < _objectives.Count
            ? _objectives[CurrentObjectiveIndex]
            : null;

    public int CompletedCount => _objectives.Count(x => x.IsCompleted);
    public int TotalCount => _objectives.Count;
    public double CompletionPercentage => TotalCount == 0 ? 0 : CompletedCount * 100.0 / TotalCount;

    public void AddObjective(string name, string location, string? imagePath = null)
    {
        _objectives.Add(new Objective
        {
            Name = name,
            Location = location,
            ImagePath = imagePath,
            Order = _objectives.Count
        });
        RefreshOrder();
    }

    public bool CompleteCurrentObjective()
    {
        if (CurrentObjective is null) return false;

        CurrentObjective.IsCompleted = true;
        CurrentObjectiveIndex = GetFirstIncompleteIndexOrEnd();
        return true;
    }

    public bool ProcessDetectedEvent(string eventType, string value)
    {
        var current = CurrentObjective;
        if (current is null) return false;

        var matches = string.Equals(current.DetectionType, eventType, StringComparison.OrdinalIgnoreCase)
            && string.Equals(current.DetectionValue, value, StringComparison.OrdinalIgnoreCase);

        return matches && CompleteCurrentObjective();
    }

    public bool GoToPreviousObjective()
    {
        for (var i = CurrentObjectiveIndex - 1; i >= 0; i--)
        {
            if (!_objectives[i].IsCompleted)
            {
                CurrentObjectiveIndex = i;
                return true;
            }
        }

        return false;
    }

    public void RefreshOrder()
    {
        var ordered = _objectives.OrderBy(x => x.Order).ThenBy(x => x.Name).ToList();
        for (var i = 0; i < ordered.Count; i++) ordered[i].Order = i;

        _objectives.Clear();
        _objectives.AddRange(ordered);
        CurrentObjectiveIndex = _objectives.Count == 0 ? 0 : GetFirstIncompleteIndexOrEnd();
    }

    private int GetFirstIncompleteIndexOrEnd()
    {
        for (var i = 0; i < _objectives.Count; i++)
        {
            if (!_objectives[i].IsCompleted) return i;
        }

        return _objectives.Count;
    }
}
