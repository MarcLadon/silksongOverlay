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
    private readonly TextBlock _completionBlock;
    private readonly ProgressBar _completionBar;

    public CurrentObjectiveWidget()
    {
        Type = "currentObjective";
        Configuration.Type = Type;

        _image = new Image
        {
            Width = 64,
            Height = 64,
            Stretch = Stretch.UniformToFill,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        _titleBlock = new TextBlock
        {
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            TextWrapping = TextWrapping.Wrap
        };

        _locationBlock = new TextBlock
        {
            FontSize = 14,
            Foreground = new SolidColorBrush(Colors.LightGray),
            TextWrapping = TextWrapping.Wrap
        };

        _completionBlock = new TextBlock
        {
            FontSize = 13,
            Foreground = new SolidColorBrush(Colors.LimeGreen),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 4, 0, 2)
        };

        _completionBar = new ProgressBar
        {
            Height = 8,
            Minimum = 0,
            Maximum = 100,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
            Foreground = new SolidColorBrush(Color.FromArgb(220, 122, 229, 130))
        };

        var textPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        textPanel.Children.Add(_titleBlock);
        textPanel.Children.Add(_locationBlock);
        textPanel.Children.Add(_completionBlock);
        textPanel.Children.Add(_completionBar);

        var grid = new Grid
        {
            Margin = new Thickness(0, 2, 0, 2)
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        Grid.SetColumn(_image, 0);
        Grid.SetColumn(textPanel, 1);

        grid.Children.Add(_image);
        grid.Children.Add(textPanel);

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(170, 17, 22, 32)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 110, 168, 254)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(10, 4, 10, 4),
            Child = grid
        };

        View = border;
        Configuration.Width = 245;
        Configuration.Height = 120;
    }

    public void Bind(OverlayViewModel viewModel)
    {
        var current = viewModel.ObjectiveService.CurrentObjective;

        if (current is null)
        {
            _titleBlock.Text = "Terminé";
            _locationBlock.Text = "Tous les objectifs sont accomplis";
            _completionBlock.Text = "Complétion : 100.00%";
            _completionBar.Value = 100;
            _image.Source = null;
            return;
        }

        _titleBlock.Text = current.Name;
        _locationBlock.Text = $"Lieu : {current.Location}";
        _completionBlock.Text = $"Complétion : {viewModel.CompletionPercentage:0.00}%";
        _completionBar.Value = viewModel.CompletionPercentage;

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
