using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using UtaChanManager.Models;

namespace UtaChanManager.Services;

public static class WindowEnumerator
{
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
    
    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);
    
    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxLength);
    
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    
    [DllImport("user32.dll")] 
    private static extern IntPtr GetForegroundWindow();
    
    [DllImport("user32.dll")] 
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    
    private const int WmGetIcon = 0x007F;
    private const int IconSmall = 0;
    private const int IconBig = 1;
    private static readonly List<string> IgnoredProcesses = ["UtaChanManager", "ApplicationFrameHost", "TextInputHost", "msedgewebview2", "explorer"];
    
    public static List<WindowInfo> GetOpenedWindows()
    {
        var windows = new List<WindowInfo>();
        
        EnumWindows((hWnd, lParam) =>
        {
            if (!IsWindowVisible(hWnd)) return true;
            
            var windowText = new StringBuilder(256);
            GetWindowText(hWnd, windowText, 256);
            var title = windowText.ToString();

            if (string.IsNullOrEmpty(title)) return true;

            GetWindowThreadProcessId(hWnd, out var id);
            try
            {
                var process = Process.GetProcessById((int) id);
                if (IgnoredProcesses.Contains(process.ProcessName)) return true;
                windows.Add(new WindowInfo
                {
                    Key = hWnd.ToString(),
                    Handle = hWnd,
                    Title = title+$" ({process.ProcessName})",
                    ProcessName = process.ProcessName,
                    Icon = GetIconForWindowInfo(process, hWnd),
                    IsActive = hWnd == GetActiveWindowHandle()
                });
            } catch { }
            return true;
        }, IntPtr.Zero);
        
        return windows;
    }
    
    private static BitmapSource? GetIconForWindowInfo(Process process, IntPtr hWnd)
    {
        BitmapSource? icon = GetIconFromWindow(hWnd) ?? GetIconFromExecutable(process);
        return icon;
    }

    private static BitmapSource? GetIconFromExecutable(Process process)
    {
        if (process.MainModule == null || string.IsNullOrWhiteSpace(process.MainModule?.FileName)) return null;
        try
        {
            var extractedIcon = Icon.ExtractAssociatedIcon(process.MainModule.FileName);
            if (extractedIcon == null) return null;
            using var iconStream = new MemoryStream();
            extractedIcon.Save(iconStream);
            return BitmapFrame.Create(iconStream);

        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    
    private static BitmapSource? GetIconFromWindow(IntPtr hWnd)
    {
        try
        {
            IntPtr hIcon = SendMessage(hWnd, WmGetIcon, (IntPtr)IconSmall, IntPtr.Zero);
            if (hIcon == IntPtr.Zero)
                hIcon = SendMessage(hWnd, WmGetIcon, (IntPtr)IconBig, IntPtr.Zero);

            if (hIcon == IntPtr.Zero) return null;
            return Imaging.CreateBitmapSourceFromHIcon(
                hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(32, 32));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    
    public static IntPtr GetActiveWindowHandle()
    {
        return GetForegroundWindow();
    }
}