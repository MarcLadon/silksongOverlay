namespace SilksongOverlay.Models;

public sealed class Objective
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = string.Empty;

    public string Detail { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string? ImagePath { get; set; }

    public string? MapImagePath { get; set; }

    public bool IsCompleted { get; set; }

    public int Order { get; set; }
}
