using System.Windows;
using System.Windows.Input;

namespace SilksongOverlay;

public partial class OverlayWindow : Window
{
    private bool _isEditMode;

    public OverlayWindow()
    {
        InitializeComponent();
        Left = 40;
        Top = 40;
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set
        {
            _isEditMode = value;
            Opacity = _isEditMode ? 0.98 : 1.0;
        }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_isEditMode)
        {
            DragMove();
        }
    }
}
