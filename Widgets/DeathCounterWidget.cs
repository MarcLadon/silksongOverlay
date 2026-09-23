using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SilksongOverlay.ViewModels;

namespace SilksongOverlay.Widgets;

public sealed class DeathCounterWidget : WidgetBase
{
    private readonly TextBlock _textBlock;

    public DeathCounterWidget()
    {
        Type = "deathCounter";
        Configuration.Type = Type;

        _textBlock = new TextBlock
        {
            Foreground = new SolidColorBrush(Colors.OrangeRed),
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(160, 17, 22, 32)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(180, 255, 107, 107)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10),
            Child = _textBlock
        };

        View = border;
        Configuration.Width = 180;
        Configuration.Height = 40;
    }

    public void Bind(OverlayViewModel viewModel)
    {
        _textBlock.Text = $"Morts : {viewModel.DeathText}";
    }

    public override void Refresh()
    {
    }
}
