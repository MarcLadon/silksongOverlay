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

    public double CompletionPercentage =>
        TotalCount == 0 ? 0 : (CompletedCount * 100.0) / TotalCount;

    public void AddObjective(string name, string location, string? imagePath = null)
    {
        var objective = new Objective
        {
            Name = name,
            Location = location,
            ImagePath = imagePath,
            Order = _objectives.Count
        };

        _objectives.Add(objective);
        RefreshOrder();
    }

    public bool CompleteCurrentObjective()
    {
        if (CurrentObjective is null)
        {
            return false;
        }

        CurrentObjective.IsCompleted = true;
        CurrentObjectiveIndex = GetFirstIncompleteIndexOrEnd();
        return true;
    }

    public bool GoToPreviousObjective()
    {
        if (_objectives.Count == 0)
        {
            return false;
        }

        for (int i = CurrentObjectiveIndex - 1; i >= 0; i--)
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
        var ordered = _objectives
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Name)
            .ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            ordered[i].Order = i;
        }

        _objectives.Clear();
        _objectives.AddRange(ordered);

        if (_objectives.Count == 0)
        {
            CurrentObjectiveIndex = 0;
            return;
        }

        CurrentObjectiveIndex = GetFirstIncompleteIndexOrEnd();
    }

    private int GetFirstIncompleteIndexOrEnd()
    {
        for (int i = 0; i < _objectives.Count; i++)
        {
            if (!_objectives[i].IsCompleted)
            {
                return i;
            }
        }

        return _objectives.Count;
    }
}
