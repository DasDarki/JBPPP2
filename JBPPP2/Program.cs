using JBPPP2.Controllers;
using JBPPP2.Model;

namespace JBPPP2;

internal static class Program
{
    [STAThread]
    internal static void Main(string[] args)
    {
        Config.Load();

        var window = new Window(args);

        window.RegisterController<DataController>();
        window.RegisterController<InstallController>();
        window.RegisterController<LauncherController>();
        window.WaitForExit();
    }
}