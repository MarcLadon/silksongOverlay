using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SilksongOverlay.ViewModels;

namespace SilksongOverlay.Widgets;

public sealed class CurrentObjectiveWidget : WidgetBase
{
    private readonly Image _image;
    private readonly TextBlock _titleBlock;
    private readonly TextBlock _locationBlock;
    private readonly ProgressBar _progressBar;

    public CurrentObjectiveWidget()
    {
        Type = "currentObjective";
        Configuration.Type = Type;

        _image = new Image
        {
            Width = 80,
            Height = 80,
            Stretch = Stretch.UniformToFill,
            Margin = new Thickness(0, 0, 12, 0)
        };

        _titleBlock = new TextBlock
        {
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            TextWrapping = TextWrapping.Wrap
        };

        _locationBlock = new TextBlock
        {
            FontSize = 16,
            Foreground = new SolidColorBrush(Colors.LightGray),
            TextWrapping = TextWrapping.Wrap
        };

        _progressBar = new ProgressBar
        {
            Height = 10,
            Minimum = 0,
            Maximum = 100,
            Margin = new Thickness(0, 8, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
            Foreground = new SolidColorBrush(Color.FromArgb(220, 110, 168, 254))
        };

        var textPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center
        };

        textPanel.Children.Add(_titleBlock);
        textPanel.Children.Add(_locationBlock);
        textPanel.Children.Add(_progressBar);

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            VerticalAlignment = VerticalAlignment.Center
        };

        panel.Children.Add(_image);
        panel.Children.Add(textPanel);

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(170, 17, 22, 32)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 110, 168, 254)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(8),
            Child = panel
        };

        View = border;
        Configuration.Width = 500;
        Configuration.Height = 150;
    }

    public void Bind(OverlayViewModel viewModel)
    {
        var current = viewModel.ObjectiveService.CurrentObjective;

        if (current is null)
        {
            _titleBlock.Text = "Terminé";
            _locationBlock.Text = "Tous les objectifs sont accomplis";
            _progressBar.Value = 100;
            _image.Source = null;
            return;
        }

        _titleBlock.Text = current.Name;
        _locationBlock.Text = $"Lieu : {current.Location}";
        _progressBar.Value = viewModel.CompletionPercentage;

        _image.Source = TryLoadImage(current.ImagePath);
    }

    private static BitmapImage? TryLoadImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return null;
        }

        try
        {
            var fullPath = Path.IsPathRooted(imagePath)
                ? imagePath
                : Path.Combine(Environment.CurrentDirectory, imagePath);

            if (!File.Exists(fullPath))
            {
                return null;
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement de l'image '{imagePath}': {ex.Message}");
            return null;
        }
    }

    public override void Refresh()
    {
    }
}
