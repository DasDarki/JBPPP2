using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using JBPPP2.Model;
using Newtonsoft.Json;

namespace JBPPP2;

internal class AutoUpdater
{
    private const string UpdateDataUrl = "https://raw.githubusercontent.com/DasDarki/JBPPP2/master/update.json";
    private static string? _cachedDownloadUrl = null;

    internal static bool CheckForUpdates()
    {
        if (Config.Instance?.SkipUpdateCheck == true)
        {
            return false;
        }
        
        var data = GetUpdateData();

        if (data == null)
        {
            return false;
        }
        
        if (data.Version > Assembly.GetExecutingAssembly().GetName().Version)
        {
            _cachedDownloadUrl = data.DownloadUrl;
            return true;
        }
        
        return false;
    }

    internal static bool Install()
    {
        if (_cachedDownloadUrl == null)
        {
            return false;
        }
        
        try
        {
            var tmp = Path.GetTempFileName();

            using var client = new WebClient();
            
            client.DownloadFile(_cachedDownloadUrl, tmp);
            
            var tmpBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(tmp));
            var updater = Path.Combine(Environment.CurrentDirectory, "updater", "JBPPP2.Updater" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : ""));
            
            var pi = new ProcessStartInfo(updater, tmpBase64 + " " + Convert.ToBase64String(Encoding.UTF8.GetBytes(Assembly.GetExecutingAssembly().Location)))
            {
                UseShellExecute = true,
                Verb = "runas"
            };
            
            Process.Start(pi);
            
            Environment.Exit(0);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    private static UpdateData? GetUpdateData()
    {
        try
        {
            using var client = new HttpClient();
            var json = client.GetStringAsync(UpdateDataUrl).GetAwaiter().GetResult();
        
            return JsonConvert.DeserializeObject<UpdateData>(json);
        }
        catch
        {
            return null;
        }
    }
}