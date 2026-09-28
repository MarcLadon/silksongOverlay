using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;
using SilksongOverlay.Core;
using SilksongOverlay.Models;
using SilksongOverlay.Services;
using SilksongOverlay.ViewModels;
using SilksongOverlay.Widgets;

namespace SilksongOverlay;

public partial class MainWindow : Window
{
    private readonly string _configPath = "config/overlay.json";
    private readonly string _csvObjectivesPath = "data/objectives.csv";
    private readonly ConfigurationService _configurationService;
    private readonly OverlayViewModel _viewModel;
    private readonly OverlayWindow _overlayWindow = new();
    private readonly List<WidgetBase> _widgets = new();
    private readonly DispatcherTimer _uiTimer = new() { Interval = TimeSpan.FromMilliseconds(200) };
    private IObjectiveDetector? _objectiveDetector;
    private HwndSource? _source;
    private readonly Dictionary<int, Action> _hotkeys = new();
    private const int WM_HOTKEY = 0x0312;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;

    public MainWindow()
    {
        InitializeComponent();
        _configurationService = new ConfigurationService(_configPath);
        var config = _configurationService.Load();

        // Charger les objectifs depuis le CSV
        var csvLoader = new CsvObjectiveLoader(_csvObjectivesPath);
        var csvObjectives = csvLoader.Load();

        if (csvObjectives.Count > 0)
        {
            config.Objectives = csvObjectives;
        }
        else if (config.Objectives.Count == 0)
        {
            // Si aucun CSV n'existe et aucun objectif sauvegardé, créer des défauts
            config.Objectives = new List<Objective>
            {
                new() { Name = "Entrer dans les quais profonds", Location = "Quais profonds", Order = 0, DetectionType = "area_entered", DetectionValue = "Deep Docks" },
                new() { Name = "Vaincre le boss", Location = "Arène", Order = 1, DetectionType = "boss_defeated", DetectionValue = "Bell Beast" }
            };
        }

        _viewModel = new OverlayViewModel(config.Objectives) { DeathCount = config.DeathCount };
        if (config.TimerRunning) _viewModel.TimerService.Start();

        _uiTimer.Tick += UiTimer_Tick;
        _uiTimer.Start();
        BuildOverlayWidgets();
        RefreshUI();
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        _source?.AddHook(WndProc);
        RegisterHotkeys();
        EnableClickThrough(true);

        var eventPath = Path.Combine(Environment.CurrentDirectory, "runtime", "game-events.json");
        StartObjectiveDetection(eventPath);
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        SaveConfiguration();
        _objectiveDetector?.Dispose();
        UnregisterHotkeys();
    }

    private void StartObjectiveDetection(string path)
    {
        _objectiveDetector = new FileObjectiveDetector(path);
        _objectiveDetector.ObjectiveDetected += ObjectiveDetector_ObjectiveDetected;
        _ = _objectiveDetector.StartAsync(CancellationToken.None);
    }

    private void ObjectiveDetector_ObjectiveDetected(object? sender, ObjectiveDetectedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (_viewModel.ProcessDetectedEvent(e.EventType, e.Value))
            {
                RefreshUI();
                SaveConfiguration();
            }
        });
    }

    private void UiTimer_Tick(object? sender, EventArgs e)
    {
        RefreshUI();
    }

    private void BuildOverlayWidgets()
    {
        var widgets = new WidgetBase[]
        {
            new TimerWidget { Configuration = { X = 20, Y = 20, Width = 220, Height = 60 } },
            new DeathCounterWidget { Configuration = { X = 20, Y = 90, Width = 180, Height = 40 } },
            new CurrentObjectiveWidget { Configuration = { X = 20, Y = 150, Width = 500, Height = 150 } },
            new CompletionWidget { Configuration = { X = 20, Y = 320, Width = 300, Height = 70 } }
        };

        _widgets.AddRange(widgets);
        foreach (var widget in _widgets)
        {
            widget.UpdateFromConfiguration();
            _overlayWindow.OverlayCanvas.Children.Add(widget.View);
        }

        _overlayWindow.Show();
    }

    private void RefreshUI()
    {
        ObjectivesList.ItemsSource = null;
        ObjectivesList.ItemsSource = _viewModel.ObjectiveService.Objectives;
        CurrentObjectiveText.Text = _viewModel.CurrentObjectiveName;
        CompletionText.Text = $"{_viewModel.CompletionPercentage:0.00}%";
        DeathsText.Text = _viewModel.DeathText;
        TimerText.Text = _viewModel.TimerText;
        RefreshOverlayWidgets();
    }

    private void RefreshOverlayWidgets()
    {
        foreach (var widget in _widgets)
        {
            switch (widget)
            {
                case TimerWidget timer: timer.Bind(_viewModel); break;
                case DeathCounterWidget deaths: deaths.Bind(_viewModel); break;
                case CurrentObjectiveWidget objective: objective.Bind(_viewModel); break;
                case CompletionWidget completion: completion.Bind(_viewModel); break;
            }
        }
    }

    private void AddObjectiveButton_Click(object sender, RoutedEventArgs e)
    {
        var name = ObjectiveNameText.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) { MessageBox.Show("Le nom de l'objectif est obligatoire."); return; }
        _viewModel.ObjectiveService.AddObjective(name, ObjectiveLocationText.Text.Trim());
        RefreshUI(); SaveConfiguration();
    }

    private void CompleteObjectiveButton_Click(object sender, RoutedEventArgs e) { _viewModel.CompleteCurrentObjective(); RefreshUI(); SaveConfiguration(); }
    private void PreviousObjectiveButton_Click(object sender, RoutedEventArgs e) { _viewModel.GoPreviousObjective(); RefreshUI(); SaveConfiguration(); }
    private void AddDeathButton_Click(object sender, RoutedEventArgs e) { _viewModel.AddDeath(); RefreshUI(); SaveConfiguration(); }
    private void ToggleTimerButton_Click(object sender, RoutedEventArgs e) { _viewModel.ToggleTimer(); RefreshUI(); SaveConfiguration(); }
    private void ResetTimerButton_Click(object sender, RoutedEventArgs e) { _viewModel.ResetTimer(); RefreshUI(); SaveConfiguration(); }

    private void ToggleOverlayButton_Click(object sender, RoutedEventArgs e)
    {
        if (_overlayWindow.IsVisible) _overlayWindow.Hide(); else _overlayWindow.Show();
    }

    private void ToggleEditModeButton_Click(object sender, RoutedEventArgs e)
    {
        _overlayWindow.IsEditMode = !_overlayWindow.IsEditMode;
        EnableClickThrough(!_overlayWindow.IsEditMode);
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
        var dialog = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp" };
        if (dialog.ShowDialog() == true && ObjectivesList.SelectedItem is Objective selected)
        {
            selected.ImagePath = dialog.FileName;
            RefreshUI(); SaveConfiguration();
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e) { SaveConfiguration(); MessageBox.Show("Configuration enregistrée."); }

    private void SaveConfiguration()
    {
        _configurationService.Save(new OverlayConfiguration
        {
            Objectives = _viewModel.ObjectiveService.Objectives.ToList(),
            CurrentObjectiveIndex = _viewModel.ObjectiveService.CurrentObjectiveIndex,
            DeathCount = _viewModel.DeathCount,
            TimerText = _viewModel.TimerText,
            TimerRunning = _viewModel.TimerService.IsRunning
        });
    }

    private void RegisterHotkeys()
    {
        RegisterHotKey(1001, KeyInterop.VirtualKeyFromKey(Key.F1), ModifierKeys.None); RegisterHotKey(1002, KeyInterop.VirtualKeyFromKey(Key.F2), ModifierKeys.None);
        RegisterHotKey(1003, KeyInterop.VirtualKeyFromKey(Key.F3), ModifierKeys.None); RegisterHotKey(1004, KeyInterop.VirtualKeyFromKey(Key.F4), ModifierKeys.None);
        RegisterHotKey(1005, KeyInterop.VirtualKeyFromKey(Key.F4), ModifierKeys.Shift); RegisterHotKey(1006, KeyInterop.VirtualKeyFromKey(Key.E), ModifierKeys.Control);
        RegisterHotKey(1007, KeyInterop.VirtualKeyFromKey(Key.O), ModifierKeys.Control);
        _hotkeys[1001] = () => { _viewModel.ToggleTimer(); RefreshUI(); SaveConfiguration(); }; _hotkeys[1002] = () => { _viewModel.ResetTimer(); RefreshUI(); SaveConfiguration(); };
        _hotkeys[1003] = () => { _viewModel.AddDeath(); RefreshUI(); SaveConfiguration(); }; _hotkeys[1004] = () => { _viewModel.CompleteCurrentObjective(); RefreshUI(); SaveConfiguration(); };
        _hotkeys[1005] = () => { _viewModel.GoPreviousObjective(); RefreshUI(); SaveConfiguration(); }; _hotkeys[1006] = () => { _overlayWindow.IsEditMode = !_overlayWindow.IsEditMode; EnableClickThrough(!_overlayWindow.IsEditMode); };
        _hotkeys[1007] = () => { if (_overlayWindow.IsVisible) _overlayWindow.Hide(); else _overlayWindow.Show(); };
    }

    private void UnregisterHotkeys()
    {
        foreach (var id in _hotkeys.Keys) UnregisterHotKey(new WindowInteropHelper(this).Handle, id);
        _hotkeys.Clear();
    }

    private void RegisterHotKey(int id, int virtualKey, ModifierKeys modifiers) => RegisterHotKeyNative(new WindowInteropHelper(this).Handle, id, (uint)modifiers, (uint)virtualKey);

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && _hotkeys.TryGetValue(wParam.ToInt32(), out var action)) action();
        return IntPtr.Zero;
    }

    private void EnableClickThrough(bool enable)
    {
        var hwnd = new WindowInteropHelper(_overlayWindow).Handle;
        if (hwnd == IntPtr.Zero) return;
        var style = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, enable ? style | WS_EX_LAYERED | WS_EX_TRANSPARENT : style & ~WS_EX_TRANSPARENT);
    }

    [DllImport("user32.dll", EntryPoint = "RegisterHotKey")] private static extern bool RegisterHotKeyNative(IntPtr hWnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int index);
    [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int index, int value);
}
