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
    private readonly Image _mapImage;
    private readonly TextBlock _titleBlock;
    private readonly TextBlock _detailBlock;
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
            Stretch = Stretch.Uniform,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        _mapImage = new Image
        {
            Width = 100,
            Height = 100,
            Stretch = Stretch.Uniform,
            Margin = new Thickness(10, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        _titleBlock = new TextBlock
        {
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        _detailBlock = new TextBlock
        {
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromRgb(255, 224, 138)),
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Visibility = Visibility.Collapsed
        };

        _locationBlock = new TextBlock
        {
            FontSize = 14,
            Foreground = new SolidColorBrush(Colors.LightGray),
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        _completionBlock = new TextBlock
        {
            FontSize = 13,
            Foreground = new SolidColorBrush(Colors.LimeGreen),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 4, 0, 2),
            HorizontalAlignment = HorizontalAlignment.Stretch
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

        var textPanel = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        for (var i = 0; i < 5; i++)
        {
            textPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        Grid.SetRow(_titleBlock, 0);
        Grid.SetRow(_detailBlock, 1);
        Grid.SetRow(_locationBlock, 2);
        Grid.SetRow(_completionBlock, 3);
        Grid.SetRow(_completionBar, 4);

        textPanel.Children.Add(_titleBlock);
        textPanel.Children.Add(_detailBlock);
        textPanel.Children.Add(_locationBlock);
        textPanel.Children.Add(_completionBlock);
        textPanel.Children.Add(_completionBar);

        var leftPanel = new Grid
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        leftPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        leftPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        Grid.SetColumn(_image, 0);
        Grid.SetColumn(textPanel, 1);

        leftPanel.Children.Add(_image);
        leftPanel.Children.Add(textPanel);

        var mainGrid = new Grid
        {
            Margin = new Thickness(0, 2, 0, 2),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(leftPanel, 0);
        Grid.SetColumn(_mapImage, 1);

        mainGrid.Children.Add(leftPanel);
        mainGrid.Children.Add(_mapImage);

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(170, 17, 22, 32)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 110, 168, 254)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(6, 2, 6, 2),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Child = mainGrid
        };

        View = border;
    }

    public override void UpdateFromConfiguration()
    {
        // Force dimensions instead of using Configuration
        View.Visibility = Configuration.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        View.Opacity = Configuration.Opacity;

        Canvas.SetLeft(View, Configuration.X);
        Canvas.SetTop(View, Configuration.Y);
        
        // Force fixed dimensions
        View.Width = 380;
        View.Height = 150;
    }

    public void Bind(OverlayViewModel viewModel)
    {
        var current = viewModel.ObjectiveService.CurrentObjective;
        var totalObjectives = viewModel.ObjectiveService.TotalCount;
        var currentIndex = viewModel.ObjectiveService.CurrentObjectiveIndex + 1;

        if (current is null)
        {
            _titleBlock.Text = "Terminé";
            _detailBlock.Text = string.Empty;
            _detailBlock.Visibility = Visibility.Collapsed;
            _locationBlock.Text = "Tous les objectifs sont accomplis";
            _completionBlock.Text = $"Complétion {totalObjectives}/{totalObjectives}";
            _completionBar.Value = 100;
            _image.Source = null;
            _mapImage.Source = null;
            return;
        }

        _titleBlock.Text = current.Name;
        _detailBlock.Text = current.Detail;
        _detailBlock.Visibility = string.IsNullOrWhiteSpace(current.Detail)
            ? Visibility.Collapsed
            : Visibility.Visible;
        _locationBlock.Text = $"Lieu : {current.Location}";
        _completionBlock.Text = $"Complétion {currentIndex}/{totalObjectives}";
        _completionBar.Value = viewModel.CompletionPercentage;
        _image.Source = TryLoadImage(current.ImagePath);
        _mapImage.Source = TryLoadImage(current.MapImagePath);
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
