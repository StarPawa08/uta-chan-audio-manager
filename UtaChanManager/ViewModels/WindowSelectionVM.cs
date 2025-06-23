using System.Collections.ObjectModel;
using System.Windows.Threading;
using UtaChanManager.Models;
using UtaChanManager.Services;
using UtaChanManager.Utils;

namespace UtaChanManager.ViewModels;

public class WindowSelectionVm
{
    private readonly DispatcherTimer _activeWindowMonitorTimer = new();
    private IntPtr _lastActiveHandle = IntPtr.Zero;
    private WindowInfo? _lastActiveWindow;

    public WindowSelectionVm()
    {
        _activeWindowMonitorTimer.Interval = TimeSpan.FromMilliseconds(500);
        _activeWindowMonitorTimer.Tick += UpdateWindowInfoTimer;
        _activeWindowMonitorTimer.Start();
    }

    private void UpdateWindowInfoTimer(object? sender, EventArgs e)
    {
        var currentHandle = WindowEnumerator.GetActiveWindowHandle();
        if (currentHandle == _lastActiveHandle) return;
        ConfigManager.Save(SelectedPriorities.ToList());
        _lastActiveHandle = currentHandle;
        Refresh();
    }

    public ObservableCollection<WindowInfo> Windows { get; set; } = new ();
    public ObservableCollection<string> SelectedPriorities
    {
        get => new(Windows.Where(w => w.IsSelected).Select(w => w.Key));
        set
        {
            foreach (var window in Windows)
            {
                window.IsSelected = value.Contains(window.Key);
            }
        }
    }

    
    public void Refresh()
    {
        Windows.Clear();
        var loadedConfig = ConfigManager.Load();
        var priorityWindowSelected = false;
        foreach (var windowInfo in WindowEnumerator.GetOpenedWindows().DistinctBy(w => w.Title))
        {
            if (loadedConfig.Contains(windowInfo.Key) || SelectedPriorities.Contains(windowInfo.Key))
            {
                windowInfo.IsSelected = true;
            }
            Windows.Add(windowInfo);
            if (!windowInfo.IsActive) continue;
            _lastActiveWindow = windowInfo;
            if (windowInfo.IsSelected)
            {
                priorityWindowSelected = true;
                _ = AudioManager.MuteAllExceptAsync(windowInfo.Id);
            }
        }
        if (!priorityWindowSelected)
        {
            _ = AudioManager.UnMuteAllAsync();
        }
    }
}