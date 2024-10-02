using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Dionysus.App.Helpers;

public class AppHelper
{
    
    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr window, int index, int value);
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_TOOLWINDOW = 0x00000080;
    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr window, int index);
    
    public static void HideFromAltTab(IntPtr windowHandle)
    {
        SetWindowLong(windowHandle, GWL_EXSTYLE,
            GetWindowLong(windowHandle, GWL_EXSTYLE) |
            WS_EX_TOOLWINDOW);
    }
    
    public static void Logic(bool value)
    {
        if (value) AddToStartup();
        else RemoveFromStartup();
    }

    static void AddToStartup()
    {
        string appPath = Application.ExecutablePath;

        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        registryKey.SetValue("Dionysus", appPath);
        registryKey.Close();
    }

    static void RemoveFromStartup()
    {
        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        if (registryKey != null)
        {
            registryKey.DeleteValue("Dionysus", false);
            registryKey.Close();
        }
    }

    public static bool AddedToStartup()
    {
        return (Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
            true).GetValueNames().Contains("Dionysus"));
    }
}