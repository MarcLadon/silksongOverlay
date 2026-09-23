using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SilksongOverlay.ViewModels;

namespace SilksongOverlay.Widgets;

public sealed class CompletionWidget : WidgetBase
{
    private readonly TextBlock _textBlock;
    private readonly ProgressBar _progressBar;

    public CompletionWidget()
    {
        Type = "completion";
        Configuration.Type = Type;

        _textBlock = new TextBlock
        {
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        _progressBar = new ProgressBar
        {
            Height = 10,
            Minimum = 0,
            Maximum = 100,
            Margin = new Thickness(0, 8, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
            Foreground = new SolidColorBrush(Color.FromArgb(220, 122, 229, 130))
        };

        var panel = new StackPanel
        {
            Margin = new Thickness(12),
            VerticalAlignment = VerticalAlignment.Center
        };

        panel.Children.Add(_textBlock);
        panel.Children.Add(_progressBar);

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(170, 17, 22, 32)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 122, 229, 130)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(8),
            Child = panel
        };

        View = border;
        Configuration.Width = 300;
        Configuration.Height = 70;
    }

    public void Bind(OverlayViewModel viewModel)
    {
        _textBlock.Text = $"Complétion : {viewModel.CompletionPercentage:0.00}%";
        _progressBar.Value = viewModel.CompletionPercentage;
    }

    public override void Refresh()
    {
    }
}
