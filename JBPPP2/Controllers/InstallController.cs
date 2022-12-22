using System.Net;
using Ionic.Zip;
using JBPPP2.Model;
using Newtonsoft.Json.Linq;

namespace JBPPP2.Controllers;

internal class InstallController
{
    [Controller("GetCurrentVersion")]
    internal static void GetCurrentVersion(Window window, string rid, string gamePath, string configFileName)
    {
        var configFile = Path.Combine(gamePath, configFileName);
        if (!File.Exists(configFile))
        {
            window.SendResult(rid, false);
            return;
        }
        
        JObject configObj = JObject.Parse(File.ReadAllText(configFile));
        var version = configObj["buildVersion"]?.ToString();
        
        window.SendResult(rid, string.IsNullOrEmpty(version) ? "" : version);
    }
    
    [Controller("ExistsNewPatch")]
    internal static void ExistsNewPatch(Window window, string rid, string gamePath, string configFileName, string newVersion)
    {
        var configFile = Path.Combine(gamePath, configFileName);
        if (!File.Exists(configFile))
        {
            window.SendResult(rid, false);
            return;
        }
        
        JObject configObj = JObject.Parse(File.ReadAllText(configFile));
        var version = configObj["buildVersion"]?.ToString();
        
        if (version == null)
        {
            window.SendResult(rid, false);
            return;
        }

        try
        {
            var change = new BuildVersion(version);
            var newChange = new BuildVersion(newVersion);

            window.SendResult(rid, newChange.IsNewerThan(change));
        }
        catch
        {
            window.SendResult(rid, false);
        }
    }
    
    [Controller("CheckVersion")]
    internal static void CheckVersion(Window window, string rid, string gamePath, string configFileName, string newVersion)
    {
        if (Config.Instance is {SkipVersionCheck: true})
        {
            window.SendResult(rid, true);
            return;
        }

        var configFile = Path.Combine(gamePath, configFileName);
        if (!File.Exists(configFile))
        {
            window.SendResult(rid, false);
            return;
        }
        
        JObject configObj = JObject.Parse(File.ReadAllText(configFile));
        var version = configObj["buildVersion"]?.ToString();

        if (string.IsNullOrEmpty(version))
        {
            window.SendResult(rid, false);
            return;
        }
        
        window.SendResult(rid, newVersion.StartsWith(version));
    }
    
    [Controller("Download")]
    internal static void Download(Window window, string progressId, string url)
    {
        var tmpFile = Path.Combine(DataController.GetDataPath(), "tmp");
        Directory.CreateDirectory(tmpFile);
        tmpFile = Path.Combine(tmpFile, Guid.NewGuid() + ".zip");

        Thread thread = new Thread(() => {
            WebClient client = new WebClient();
            client.DownloadProgressChanged += (_, args) => {
                double bytesIn = args.BytesReceived;
                double totalBytes = args.TotalBytesToReceive;
                double percentage = bytesIn / totalBytes;

                window.SendCommand("DownloadProgress", progressId, percentage);
            };
            client.DownloadFileCompleted += (_, _) => {
                window.SendCommand("DownloadFinish", progressId, tmpFile);
            };
            client.DownloadFileAsync(new Uri(url), tmpFile);
        }){IsBackground = true};
        thread.Start();
    }
    
    [Controller("Install")]
    internal static void Install(Window window, string id, string zip, string destination, bool skipBackup)
    {
        Thread thread = new Thread(() =>
        {
            using var zipFile = new ZipFile(zip);
            var files = 0;
            var total = zipFile.Count;
            window.SendCommand("InstallProgress", id, "EXTRACT", 0, total);

            zipFile.ExtractProgress += (_, args) =>
            {
                if (args.EventType == ZipProgressEventType.Extracting_BeforeExtractEntry)
                {
                    window.SendCommand("InstallProgress", id, "EXTRACT", ++files, total);
                }
            };

            var extractTo = GetTempFolder(id);

            zipFile.ExtractAll(extractTo, ExtractExistingFileAction.OverwriteSilently);

            files = 0;
            window.SendCommand("InstallProgress", id, "MOVE", 0, total);

            void MoveFiles(string sourceRoot, string destRoot)
            {
                foreach (var innerDir in Directory.GetDirectories(sourceRoot))
                {
                    var dirName = Path.GetFileName(innerDir);
                    var destDir = Path.Combine(destRoot, dirName);
                    Directory.CreateDirectory(destDir);

                    MoveFiles(innerDir, destDir);
                }

                foreach (var file in Directory.GetFiles(sourceRoot))
                {
                    var fileName = Path.GetFileName(file);
                    var destFile = Path.Combine(destRoot, fileName);

                    ReplaceFileAndBackup(file, destFile, skipBackup);
                    files++;

                    window.SendCommand("InstallProgress", id, "MOVE", files, total);
                }
            }

            MoveFiles(extractTo, destination);

            window.SendCommand("InstallProgress", id, "CLEAN", total, total);

            var path = Path.Combine(destination, "patched");
            File.WriteAllText(path, files.ToString());

            Directory.Delete(extractTo, true);

            window.SendCommand("InstallFinish", id);
        }) {IsBackground = true};
        thread.Start();
    }

    [Controller("Uninstall")]
    internal static void Uninstall(Window window, string id, string gameFolder)
    {
        var patchedFile = Path.Combine(gameFolder, "patched");
        if (!File.Exists(patchedFile))
        {
            window.SendCommand("UninstallProgress", id, "ERR01");
            return;
        }
        
        if (!int.TryParse(File.ReadAllText(patchedFile), out var files))
        {
            window.SendCommand("UninstallProgress", id, "ERR02");
            return;
        }
        
        var count = 0;
        window.SendCommand("UninstallProgress", id, "MOVE", 0, files);

        void RestoreBackup(string dir)
        {
            foreach (var file in Directory.GetFiles(dir))
            {
                var fileName = Path.GetFileName(file);
                var backupFile = Path.Combine(dir, fileName + ".jbppp2_bak");
                if (File.Exists(backupFile))
                {
                    File.Move(backupFile, file, true);
                    File.Delete(backupFile);
                    window.SendCommand("UninstallProgress", id!, "MOVE", ++count, files);
                }
            }
            
            foreach (var innerDir in Directory.GetDirectories(dir))
            {
                RestoreBackup(innerDir);
            }
        }
        
        RestoreBackup(gameFolder);
        
        File.Delete(patchedFile);
        
        window.SendCommand("UninstallFinish", id);
    }
    
    [Controller("IsPatchInstalled")]
    internal static void IsPatchInstalled(Window window, string rid, string gameFolder)
    {
        var patchedFile = Path.Combine(gameFolder, "patched");
        window.SendResult(rid, File.Exists(patchedFile));
    }

    private static void ReplaceFileAndBackup(string source, string destination, bool skipBackup)
    {
        if (!skipBackup && File.Exists(destination))
        {
            File.Copy(destination, destination + ".jbppp2_bak", true);
        }
        
        File.Move(source, destination, true);
    }

    private static string GetTempFolder(string rid)
    {
        var path = Path.Combine(DataController.GetDataPath(), "tmp", rid);
        Directory.CreateDirectory(path);
        return path;
    }
    
}