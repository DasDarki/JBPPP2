using System.Diagnostics;
using System.Runtime.InteropServices;
using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using Gameloop.Vdf.Linq;
using JBPPP2.Extensions;
using JBPPP2.Model;
using JBPPP2.Model.Enums;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace JBPPP2.Controllers;

internal class LauncherController
{
    [Controller("GetLaunchers")]
    internal static void GetLaunchers(Window window, string rid)
    {
        if (Config.Instance is {Launchers.Count: > 0})
        {
            window.SendResult(rid, Config.Instance.Launchers);
            return;
        }

        var launchers = FindLaunchers();
        window.SendResult(rid, launchers != null ? launchers : false);
    }
    
    [Controller("SaveLaunchers")]
    internal static void SaveLaunchers(Window _, JArray launchers)
    {
        if (Config.Instance == null)
        {
            return;
        }
        
        Config.Instance.Launchers = launchers.ToList<Launcher>();
        Config.Instance.Save();
    }

    [Controller("FindGamePath")]
    internal static void FindGamePath(Window window, string rid, long priorityVal, long appid, string appname)
    {
        LauncherType priority = (LauncherType) priorityVal;
        
        string? path = priority switch
        {
            LauncherType.Epic => FindGamePathOnEpic(appname),
            LauncherType.Steam => FindGamePathOnSteam(appid),
            _ => null
        };
        
        if (path != null)
        {
            window.SendResult(rid, path);
            return;
        }
        
        window.SendResult(rid, priority switch
        {
            LauncherType.Epic => FindGamePathOnSteam(appid),
            LauncherType.Steam => FindGamePathOnEpic(appname),
            _ => null
        });
    }

    [Controller("LaunchGame")]
    internal static void LaunchGame(Window window, string rid, long appid)
    {
        if (string.IsNullOrEmpty(FindGamePathOnSteam(appid)))
        {
            window.SendResult(rid, false);
            return;
        }
        
        Process.Start(new ProcessStartInfo("steam://launch/" + appid + "/dialog")
        {
            UseShellExecute = true,
            Verb = "open"
        });
        
        window.SendResult(rid, true);
    }

    private static string? FindGamePathOnSteam(long appid)
    {
        if (Config.Instance == null)
        {
            return null;
        }
        
        var steamPath = Config.Instance.Launchers.FirstOrDefault(x => x.Type == LauncherType.Steam)?.Path;
        
        if (string.IsNullOrEmpty(steamPath) || steamPath.StartsWith("<REPLACE"))
        {
            return null;
        }
        
        var path = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        
        if (!File.Exists(path))
        {
            return null;
        }
        
        VProperty vdf = VdfConvert.Deserialize(File.ReadAllText(path));

        foreach (var val in vdf.Value.Children())
        {
            if (val is not VProperty prop)
            {
                continue;
            }

            if (prop.Value is not VObject obj)
            {
                continue;
            }

            var libPath = obj["path"]?.Value<string>();
            
            if (string.IsNullOrEmpty(libPath))
            {
                continue;
            }

            var appManifest = Path.Combine(libPath, "steamapps", "appmanifest_" + appid + ".acf");
            
            if (!File.Exists(appManifest))
            {
                continue;
            }
            
            VProperty manifest = VdfConvert.Deserialize(File.ReadAllText(appManifest));
            var installDir = manifest.Value["installdir"]?.Value<string>();
            
            if (string.IsNullOrEmpty(installDir))
            {
                continue;
            }
            
            return Path.Combine(libPath, "steamapps", "common", installDir);
        }

        return null;
    }

    private static string? FindGamePathOnEpic(string appname)
    {
        if (string.IsNullOrEmpty(appname))
        {
            return null;
        }

        if (Config.Instance == null)
        {
            return null;
        }
        
        var epicPath = Config.Instance.Launchers.FirstOrDefault(x => x.Type == LauncherType.Epic)?.Path;
        
        if (string.IsNullOrEmpty(epicPath) || epicPath.StartsWith("<REPLACE"))
        {
            return null;
        }
        
        epicPath = Path.Combine(epicPath, "..", "..", "UnrealEngineLauncher", "LauncherInstalled.dat");
        JObject json = JObject.Parse(File.ReadAllText(epicPath));
        var games = json["InstallationList"]?.Children().ToList();
        
        if (games == null)
        {
            return null;
        }
        
        foreach (var game in games)
        {
            var name = game["AppName"]?.Value<string>();
            
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }
            
            if (name.ToLower() != appname.ToLower())
            {
                continue;
            }
            
            var installLocation = game["InstallLocation"]?.Value<string>();
            
            if (string.IsNullOrEmpty(installLocation))
            {
                continue;
            }
            
            return installLocation;
        }

        return null;
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