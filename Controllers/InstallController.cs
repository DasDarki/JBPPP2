using Ionic.Zip;
using JBPPP2.Extensions;

namespace JBPPP2.Controllers;

internal class InstallController
{
    [Controller("Download")]
    internal static void Download(Window window, string progressId, string url)
    {
        var tmpFile = Path.Combine(DataController.GetDataPath(), "tmp");
        Directory.CreateDirectory(tmpFile);
        tmpFile = Path.Combine(tmpFile, Guid.NewGuid() + ".zip");

        Task.Run(async () =>
        {
            using var client = new HttpClient();
            using var file = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None);
            var progress = new Progress<float>(x => window.SendCommand("DownloadProgress", progressId, x));
            
            await client.DownloadAsync(url, file, progress);
            
            window.SendCommand("DownloadFinish", progressId, tmpFile);
        });
    }
    
    [Controller("Install")]
    internal static void Install(Window window, string id, string zip, string destination)
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
                
                ReplaceFileAndBackup(file, destFile);
                
                window.SendCommand("InstallProgress", id, "MOVE", ++files, total);
            }
        }
        
        MoveFiles(extractTo, destination);
        
        window.SendCommand("InstallProgress", id, "CLEAN", total, total);
        
        var path = Path.Combine(destination, "patched");
        File.WriteAllText(path, files.ToString());

        Directory.Delete(extractTo, true);
        
        window.SendCommand("InstallFinish", id);
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
        
        var total = files;
        window.SendCommand("UninstallProgress", id, "MOVE", 0, total);

        void RestoreBackup(string dir)
        {
            foreach (var file in Directory.GetFiles(dir))
            {
                var fileName = Path.GetFileName(file);
                var backupFile = Path.Combine(dir, fileName + ".bak");
                if (File.Exists(backupFile))
                {
                    File.Move(backupFile, file, true);
                    File.Delete(backupFile);
                }
                
                window.SendCommand("UninstallProgress", id!, "MOVE", ++files, total);
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

    private static void ReplaceFileAndBackup(string source, string destination)
    {
        if (File.Exists(destination))
        {
            File.Copy(destination, destination + ".bak", true);
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