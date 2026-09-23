namespace SilksongOverlay.Models;

public sealed class WidgetConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Type { get; set; } = string.Empty;

    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public double Opacity { get; set; } = 1.0;

    public bool IsVisible { get; set; } = true;

    public bool IsLocked { get; set; } = false;

    public string? FontFamily { get; set; }

    public double FontSize { get; set; } = 18;

    public string TextColor { get; set; } = "#FFFFFF";
}
