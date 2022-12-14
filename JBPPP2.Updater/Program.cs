using System.Diagnostics;
using System.Text;
using Ionic.Zip;
using System;
using System.Runtime.InteropServices;

if (args.Length != 2)
{
    return;
}

var tmpBase64 = args[0];
var tmp = Encoding.UTF8.GetString(Convert.FromBase64String(tmpBase64));

if (!File.Exists(tmp))
{
    return;
}

var exe = Encoding.UTF8.GetString(Convert.FromBase64String(args[1]));

if (!File.Exists(exe))
{
    return;
}

if (exe.EndsWith(".dll"))
{
    exe = string.Concat(exe.AsSpan(0, exe.Length - 4), RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "");
}

Console.Title = "JBPPP2 Updater (c) DasDarki";

Console.WriteLine("Installing Update! Please wait...");

var outputDir = Environment.CurrentDirectory;

var outputDirName = new DirectoryInfo(outputDir).Name;
if (outputDirName == "updater")
{
    outputDir = Path.Combine(outputDir, "..");
}

try
{
    using var zipFile = new ZipFile(tmp);
    zipFile.ExtractAll(outputDir, ExtractExistingFileAction.OverwriteSilently);

    try
    {
        File.Delete(tmp);
    } 
    catch
    {
        // ignore
    }

    var pi = new ProcessStartInfo(exe)
    {
        UseShellExecute = true
    };
    
    Process.Start(pi);
    Environment.Exit(0);
}
catch (Exception e)
{
    Console.WriteLine("Error while installing update: " + e.Message);
    
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}