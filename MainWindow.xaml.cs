using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Microsoft.Win32;
using SilksongOverlay.Core;
using SilksongOverlay.Models;
using SilksongOverlay.ViewModels;
using SilksongOverlay.Widgets;

namespace SilksongOverlay;

public partial class MainWindow : Window
{
    private readonly string _configPath = "config/overlay.json";
    private readonly ConfigurationService _configurationService;
    private readonly OverlayViewModel _viewModel;
    private readonly OverlayWindow _overlayWindow = new();
    private readonly List<WidgetBase> _widgets = new();
    private readonly DispatcherTimer _uiTimer = new();

    private HwndSource? _source;
    private bool _clickThroughEnabled = true;

    public MainWindow()
    {
        InitializeComponent();

        _configurationService = new ConfigurationService(_configPath);

        var config = _configurationService.Load();

        if (config.Objectives.Count == 0)
        {
            config.Objectives = new List<Objective>
            {
                new() { Name = "Premier objectif", Location = "Départ", Order = 0 },
                new() { Name = "Second objectif", Location = "Zone 2", Order = 1 },
                new() { Name = "Troisième objectif", Location = "Boss", Order = 2 }
            };
        }

        _viewModel = new OverlayViewModel(config.Objectives)
        {
            DeathCount = config.DeathCount
        };

        if (config.TimerRunning)
        {
            _viewModel.TimerService.Start();
        }

        _uiTimer.Interval = TimeSpan.FromMilliseconds(200);
        _uiTimer.Tick += UiTimer_Tick;
        _uiTimer.Start();

        BuildOverlayWidgets();
        RefreshUI();

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        _source = HwndSource.FromHwnd(hwnd);
        _source?.AddHook(WndProc);

        RegisterHotkeys();
        EnableClickThrough(_clickThroughEnabled);
    }

    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        SaveConfiguration();
        UnregisterHotkeys();
    }

    private void UiTimer_Tick(object? sender, EventArgs e)
    {
        if (_viewModel.TimerService.IsRunning)
        {
            RefreshUI();
            RefreshOverlayWidgets();
        }
    }

    private void BuildOverlayWidgets()
    {
        var timerWidget = new TimerWidget();
        timerWidget.Configuration.X = 20;
        timerWidget.Configuration.Y = 20;
        timerWidget.Configuration.Width = 220;
        timerWidget.Configuration.Height = 60;

        var deathWidget = new DeathCounterWidget();
        deathWidget.Configuration.X = 20;
        deathWidget.Configuration.Y = 90;
        deathWidget.Configuration.Width = 180;
        deathWidget.Configuration.Height = 40;

        var objectiveWidget = new CurrentObjectiveWidget();
        objectiveWidget.Configuration.X = 20;
        objectiveWidget.Configuration.Y = 150;
        objectiveWidget.Configuration.Width = 500;
        objectiveWidget.Configuration.Height = 150;

        var completionWidget = new CompletionWidget();
        completionWidget.Configuration.X = 20;
        completionWidget.Configuration.Y = 320;
        completionWidget.Configuration.Width = 300;
        completionWidget.Configuration.Height = 70;

        _widgets.Clear();
        _widgets.Add(timerWidget);
        _widgets.Add(deathWidget);
        _widgets.Add(objectiveWidget);
        _widgets.Add(completionWidget);

        foreach (var widget in _widgets)
        {
            widget.Configuration.IsVisible = true;
            widget.UpdateFromConfiguration();
            _overlayWindow.OverlayCanvas.Children.Add(widget.View);
        }

        _overlayWindow.Show();
    }

    private void RefreshOverlayWidgets()
    {
        foreach (var widget in _widgets)
        {
            switch (widget.Type)
            {
                case "timer":
                    ((TimerWidget)widget).Bind(_viewModel);
                    break;

                case "deathCounter":
                    ((DeathCounterWidget)widget).Bind(_viewModel);
                    break;

                case "currentObjective":
                    ((CurrentObjectiveWidget)widget).Bind(_viewModel);
                    break;

                case "completion":
                    ((CompletionWidget)widget).Bind(_viewModel);
                    break;
            }
        }
    }

    private void RefreshUI()
    {
        ObjectivesList.ItemsSource = null;
        ObjectivesList.ItemsSource = _viewModel.ObjectiveService.Objectives;

        var current = _viewModel.ObjectiveService.CurrentObjective;

        if (current != null)
        {
            CurrentObjectiveText.Text = current.Name;
        }
        else
        {
            CurrentObjectiveText.Text = "Terminé";
        }

        CompletionText.Text = $"{_viewModel.CompletionPercentage:0.00}%";
        DeathsText.Text = _viewModel.DeathText;
        TimerText.Text = _viewModel.TimerText;

        RefreshOverlayWidgets();
    }

    private void AddObjectiveButton_Click(object sender, RoutedEventArgs e)
    {
        var name = ObjectiveNameText.Text.Trim();
        var location = ObjectiveLocationText.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Le nom de l'objectif est obligatoire.");
            return;
        }

        _viewModel.ObjectiveService.AddObjective(name, location);
        RefreshUI();
        SaveConfiguration();
    }

    private void CompleteObjectiveButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CompleteCurrentObjective();
        RefreshUI();
        SaveConfiguration();
    }

    private void PreviousObjectiveButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.GoPreviousObjective();
        RefreshUI();
        SaveConfiguration();
    }

    private void AddDeathButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.AddDeath();
        RefreshUI();
        SaveConfiguration();
    }

    private void ToggleTimerButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ToggleTimer();
        RefreshUI();
        SaveConfiguration();
    }

    private void ResetTimerButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ResetTimer();
        RefreshUI();
        SaveConfiguration();
    }

    private void ToggleOverlayButton_Click(object sender, RoutedEventArgs e)
    {
        if (_overlayWindow.IsVisible)
        {
            _overlayWindow.Hide();
        }
        else
        {
            _overlayWindow.Show();
        }
    }

    private void ToggleEditModeButton_Click(object sender, RoutedEventArgs e)
    {
        _overlayWindow.IsEditMode = !_overlayWindow.IsEditMode;

        if (!_overlayWindow.IsEditMode)
        {
            EnableClickThrough(true);
        }
        else
        {
            EnableClickThrough(false);
        }
    }

    private void ObjectivesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ObjectivesList.SelectedItem is Objective objective)
        {
            ObjectiveNameText.Text = objective.Name;
            ObjectiveLocationText.Text = objective.Location;
        }
    }

    private void SelectImageButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp"
        };

        if (dialog.ShowDialog() == true)
        {
            var selected = ObjectivesList.SelectedItem as Objective;

            if (selected is null)
            {
                MessageBox.Show("Sélectionne un objectif.");
                return;
            }

            selected.ImagePath = dialog.FileName;
            RefreshUI();
            SaveConfiguration();
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveConfiguration();
        MessageBox.Show("Configuration enregistrée.");
    }

    private void SaveConfiguration()
    {
        var config = new OverlayConfiguration
        {
            Objectives = _viewModel.ObjectiveService.Objectives.ToList(),
            CurrentObjectiveIndex = _viewModel.ObjectiveService.CurrentObjectiveIndex,
            DeathCount = _viewModel.DeathCount,
            TimerText = _viewModel.TimerText,
            TimerRunning = _viewModel.TimerService.IsRunning
        };

        Directory.CreateDirectory(Path.GetDirectoryName(_configPath) ?? ".");
        _configurationService.Save(config);
    }

    // ------------------------------
    // Hotkeys
    // ------------------------------

    private const int WM_HOTKEY = 0x0312;
    private readonly Dictionary<int, Action> _hotkeys = new();

    private void RegisterHotkeys()
    {
        RegisterHotKey(1001, KeyInterop.VirtualKeyFromKey(Key.F1), ModifierKeys.None);
        RegisterHotKey(1002, KeyInterop.VirtualKeyFromKey(Key.F2), ModifierKeys.None);
        RegisterHotKey(1003, KeyInterop.VirtualKeyFromKey(Key.F3), ModifierKeys.None);
        RegisterHotKey(1004, KeyInterop.VirtualKeyFromKey(Key.F4), ModifierKeys.None);
        RegisterHotKey(1005, KeyInterop.VirtualKeyFromKey(Key.F4), ModifierKeys.Shift);
        RegisterHotKey(1006, KeyInterop.VirtualKeyFromKey(Key.E), ModifierKeys.Control);
        RegisterHotKey(1007, KeyInterop.VirtualKeyFromKey(Key.O), ModifierKeys.Control);

        _hotkeys[1001] = () =>
        {
            _viewModel.ToggleTimer();
            RefreshUI();
            SaveConfiguration();
        };

        _hotkeys[1002] = () =>
        {
            _viewModel.ResetTimer();
            RefreshUI();
            SaveConfiguration();
        };

        _hotkeys[1003] = () =>
        {
            _viewModel.AddDeath();
            RefreshUI();
            SaveConfiguration();
        };

        _hotkeys[1004] = () =>
        {
            _viewModel.CompleteCurrentObjective();
            RefreshUI();
            SaveConfiguration();
        };

        _hotkeys[1005] = () =>
        {
            _viewModel.GoPreviousObjective();
            RefreshUI();
            SaveConfiguration();
        };

        _hotkeys[1006] = () =>
        {
            _overlayWindow.IsEditMode = !_overlayWindow.IsEditMode;

            if (!_overlayWindow.IsEditMode)
            {
                EnableClickThrough(true);
            }
            else
            {
                EnableClickThrough(false);
            }
        };

        _hotkeys[1007] = () =>
        {
            if (_overlayWindow.IsVisible)
            {
                _overlayWindow.Hide();
            }
            else
            {
                _overlayWindow.Show();
            }
        };
    }

    private void UnregisterHotkeys()
    {
        foreach (var id in _hotkeys.Keys)
        {
            UnregisterHotKey(new WindowInteropHelper(this).Handle, id);
        }

        _hotkeys.Clear();
    }

    private void RegisterHotKey(int id, int virtualKey, ModifierKeys modifiers)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        RegisterHotKey(hwnd, id, (uint)modifiers, (uint)virtualKey);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            int id = wParam.ToInt32();

            if (_hotkeys.TryGetValue(id, out var action))
            {
                action();
            }
        }

        return IntPtr.Zero;
    }

    // ------------------------------
    // Click-through support
    // ------------------------------

    private void EnableClickThrough(bool enable)
    {
        var hwnd = new WindowInteropHelper(_overlayWindow).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

        if (enable)
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }
        else
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle & ~WS_EX_TRANSPARENT);
        }
    }

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;
}
