using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using SilksongOverlay.Core;
using SilksongOverlay.Models;
using SilksongOverlay.Services;
using SilksongOverlay.ViewModels;
using SilksongOverlay.Widgets;

namespace SilksongOverlay;

public partial class OverlayWindow : Window
{
    private bool _isEditMode;
    private readonly string _configPath = "config/overlay.json";
    private readonly string _csvObjectivesPath = "data/objectives.csv";
    private readonly ConfigurationService _configurationService;
    private readonly OverlayViewModel _viewModel;
    private readonly List<WidgetBase> _widgets = new();
    private IObjectiveDetector? _objectiveDetector;
    private HwndSource? _source;
    private readonly Dictionary<int, Action> _hotkeys = new();
    private const int WM_HOTKEY = 0x0312;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;

    public OverlayWindow()
    {
        InitializeComponent();
        _configurationService = new ConfigurationService(_configPath);
        var config = _configurationService.Load();

        // Charger les objectifs depuis le CSV
        var csvLoader = new CsvObjectiveLoader(_csvObjectivesPath);
        var csvObjectives = csvLoader.Load();

        if (csvObjectives.Count > 0)
        {
            // Le CSV définit la liste des objectifs, mais la progression (IsCompleted)
            // vient de la sauvegarde : on la réapplique en utilisant Order comme clé unique.
            RestoreProgress(csvObjectives, config.Objectives);
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
        _viewModel.TimerService.RestoreElapsed(TimeSpan.FromSeconds(config.TimerElapsedSeconds));
        if (config.TimerRunning) _viewModel.TimerService.Start();

        BuildOverlayWidgets();
        RefreshUI();
        PositionTopRight();
        Loaded += OverlayWindow_Loaded;
        Closing += OverlayWindow_Closing;
    }

    /// <summary>
    /// Réapplique l'état "terminé" sauvegardé sur les objectifs du CSV.
    /// L'Order est unique pour chaque objectif, c'est donc la meilleure clé de correspondance.
    /// </summary>
    private static void RestoreProgress(List<Objective> csvObjectives, List<Objective> savedObjectives)
    {
        // Créer un dictionnaire des états sauvegardés indexés par Order
        var savedState = savedObjectives
            .GroupBy(o => o.Order)
            .ToDictionary(g => g.Key, g => g.First().IsCompleted);

        // Réappliquer l'état sauvegardé à chaque objectif du CSV
        foreach (var objective in csvObjectives)
        {
            objective.IsCompleted = savedState.TryGetValue(objective.Order, out var isCompleted) && isCompleted;
        }
    }

    private void OverlayWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        _source?.AddHook(WndProc);
        RegisterHotkeys();
        EnableClickThrough(true);

        var eventPath = Path.Combine(Environment.CurrentDirectory, "runtime", "game-events.json");
        StartObjectiveDetection(eventPath);
    }

    private void OverlayWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        SaveConfiguration();
        _objectiveDetector?.Dispose();
        UnregisterHotkeys();
    }

    private void PositionTopRight()
    {
        const double margin = 40;
        var workArea = SystemParameters.WorkArea;

        Left = workArea.Right - Width - margin;
        Top = workArea.Top + margin;
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

    private void BuildOverlayWidgets()
    {
        var widgets = new WidgetBase[]
        {
            new CurrentObjectiveWidget { Configuration = { X = 20, Y = 20 } }
        };

        _widgets.AddRange(widgets);
        foreach (var widget in _widgets)
        {
            widget.UpdateFromConfiguration();
            OverlayCanvas.Children.Add(widget.View);
        }
    }

    private void RefreshUI()
    {
        foreach (var widget in _widgets)
        {
            switch (widget)
            {
                case CurrentObjectiveWidget objective: objective.Bind(_viewModel); break;
            }
        }
    }

    private void SaveConfiguration()
    {
        _configurationService.Save(new OverlayConfiguration
        {
            Objectives = _viewModel.ObjectiveService.Objectives.ToList(),
            CurrentObjectiveIndex = _viewModel.ObjectiveService.CurrentObjectiveIndex,
            DeathCount = _viewModel.DeathCount,
            TimerElapsedSeconds = _viewModel.TimerService.Elapsed.TotalSeconds,
            TimerRunning = _viewModel.TimerService.IsRunning
        });
    }

    private void RegisterHotkeys()
    {
        // F3: Objectif précédent
        RegisterHotKey(1003, KeyInterop.VirtualKeyFromKey(Key.F3), ModifierKeys.None);
        // F4: Objectif suivant
        RegisterHotKey(1004, KeyInterop.VirtualKeyFromKey(Key.F4), ModifierKeys.None);
        // F8: Quitter l'application
        RegisterHotKey(1008, KeyInterop.VirtualKeyFromKey(Key.F8), ModifierKeys.None);
        // Ctrl+E: Mode édition
        RegisterHotKey(1006, KeyInterop.VirtualKeyFromKey(Key.E), ModifierKeys.Control);
        // Ctrl+O: Show/Hide overlay
        RegisterHotKey(1007, KeyInterop.VirtualKeyFromKey(Key.O), ModifierKeys.Control);

        _hotkeys[1003] = () => { _viewModel.GoPreviousObjective(); RefreshUI(); SaveConfiguration(); };
        _hotkeys[1004] = () => { _viewModel.CompleteCurrentObjective(); RefreshUI(); SaveConfiguration(); };
        _hotkeys[1008] = () => { Dispatcher.Invoke(() => Close()); };
        _hotkeys[1006] = () => { IsEditMode = !IsEditMode; EnableClickThrough(!IsEditMode); };
        _hotkeys[1007] = () => { if (IsVisible) Hide(); else Show(); };
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
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero) return;
        var style = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, enable ? style | WS_EX_LAYERED | WS_EX_TRANSPARENT : style & ~WS_EX_TRANSPARENT);
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_isEditMode)
        {
            DragMove();
        }
    }

    [DllImport("user32.dll", EntryPoint = "RegisterHotKey")] private static extern bool RegisterHotKeyNative(IntPtr hWnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int index);
    [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int index, int value);
}
