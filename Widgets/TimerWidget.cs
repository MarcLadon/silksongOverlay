using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SilksongOverlay.ViewModels;

namespace SilksongOverlay.Widgets;

public sealed class TimerWidget : WidgetBase
{
    private readonly TextBlock _textBlock;

    public TimerWidget()
    {
        Type = "timer";
        Configuration.Type = Type;

        _textBlock = new TextBlock
        {
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 30,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(160, 17, 22, 32)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 110, 168, 254)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12),
            Child = _textBlock
        };

        View = border;
        Configuration.Width = 220;
        Configuration.Height = 60;
    }

    public void Bind(OverlayViewModel viewModel)
    {
        _textBlock.Text = viewModel.TimerText;
    }

    public override void Refresh()
    {
    }
}
