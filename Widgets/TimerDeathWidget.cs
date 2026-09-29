using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SilksongOverlay.ViewModels;

namespace SilksongOverlay.Widgets;

public sealed class TimerDeathWidget : WidgetBase
{
    private readonly TextBlock _timerBlock;
    private readonly TextBlock _deathBlock;

    public TimerDeathWidget()
    {
        Type = "timerDeath";
        Configuration.Type = Type;

        _timerBlock = new TextBlock
        {
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        _deathBlock = new TextBlock
        {
            Foreground = new SolidColorBrush(Colors.OrangeRed),
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(2, 4, 2, 4)
        };

        panel.Children.Add(_timerBlock);
        panel.Children.Add(new Separator
        {
            Width = 2,
            Margin = new Thickness(6, 0, 6, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255))
        });
        panel.Children.Add(_deathBlock);

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(160, 17, 22, 32)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 110, 168, 254)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(4, 2, 4, 2),
            Child = panel
        };

        View = border;
        Configuration.Width = 200;
        Configuration.Height = 60;
    }

    public void Bind(OverlayViewModel viewModel)
    {
        _timerBlock.Text = viewModel.TimerText;
        _deathBlock.Text = $"Morts : {viewModel.DeathText}";
    }

    public override void Refresh()
    {
    }
}
