using System.Windows;
using System.Windows.Controls;
using SilksongOverlay.Models;

namespace SilksongOverlay.Widgets;

public abstract class WidgetBase
{
    public string Id { get; } = Guid.NewGuid().ToString();

    public string Type { get; protected set; } = string.Empty;

    public WidgetConfiguration Configuration { get; set; } = new();

    public FrameworkElement View { get; protected set; }

    public bool IsVisible
    {
        get => Configuration.IsVisible;
        set => Configuration.IsVisible = value;
    }

    protected WidgetBase()
    {
        View = new Border();
    }

    public virtual void UpdateFromConfiguration()
    {
        View.Visibility = Configuration.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        View.Opacity = Configuration.Opacity;

        Canvas.SetLeft(View, Configuration.X);
        Canvas.SetTop(View, Configuration.Y);
        View.Width = Configuration.Width;
        View.Height = Configuration.Height;
    }

    public abstract void Refresh();
}
