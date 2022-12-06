using System.Runtime.InteropServices;
using JBPPP2.Model;
using JBPPP2.Model.Enums;
using Microsoft.Win32;

namespace JBPPP2.Controllers;

internal class LauncherController
{
    [Controller("GetLaunchers")]
    internal static void GetLaunchers(Window window, string rid)
    {
        if (Config.Instance != null && Config.Instance.Launchers.Count > 0)
        {
            window.SendResult(rid, Config.Instance.Launchers);
            return;
        }

        var launchers = FindLaunchers();
        window.SendResult(rid, launchers != null ? launchers : false);
    }
    
    [Controller("SaveLaunchers")]
    internal static void SaveLaunchers(Window _, List<Launcher> launchers)
    {
        if (Config.Instance == null)
        {
            return;
        }
        
        Config.Instance.Launchers = launchers;
        Config.Instance.Save();
    }

    private static List<Launcher>? FindLaunchers()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return null;
        }
        
        var launchers = new List<Launcher>();

        var steam = FindSteam();
        if (steam != null) 
            launchers.Add(steam);
        
        var epic = FindEpic();
        if (epic != null) 
            launchers.Add(epic);
        
        if (launchers.Count == 0)
        {
            return null;
        }
        
        return launchers;
    }

    private static Launcher? FindSteam()
    {
        using var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Valve\\Steam");
        if (key == null)
        {
            return null;
        }
        
        var path = key.GetValue("InstallPath") as string;
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        
        return new Launcher
        {
            Type = LauncherType.Steam,
            Path = path
        };
    }
    
    private static Launcher? FindEpic()
    {
        using var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Epic Games\\EpicGamesLauncher");
        if (key == null)
        {
            return null;
        }
        
        var path = key.GetValue("AppDataPath") as string;
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        
        return new Launcher
        {
            Type = LauncherType.Epic,
            Path = path
        };
    }
}