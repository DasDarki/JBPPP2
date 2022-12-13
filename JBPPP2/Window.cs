using JBPPP2.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PhotinoNET;
using PhotinoNET.Server;

namespace JBPPP2;

internal class Window
{
    private readonly PhotinoWindow _handle;

    internal Window(string[] args)
    {
        PhotinoServer.CreateStaticFileServer(args, out var baseUrl).RunAsync();
        
        _handle = new PhotinoWindow()
            .SetTitle("JBPPP2 (c) DasDarki")
            .Center()
            .SetResizable(true)
            .SetIconFile(Path.Combine(Environment.CurrentDirectory, "icon.ico"))
            .RegisterWebMessageReceivedHandler(OnMessageReceived);

#if DEBUG
        _handle.Load(new Uri("http://localhost:5173"));
#else
        _handle.Load($"{baseUrl}/index.html");
#endif
    }

    internal void SendResult(string rid, object? result)
    {
        SendCommand("SendResult", rid, result);
    }

    internal void WaitForExit()
    {
        _handle.WaitForClose();
    }

    internal void RegisterController<T>()
    {
        Controller.Scan<T>();
    }
    
    internal void SendCommand(string command, params object?[] args)
    {
        _handle.SendWebMessage(JsonConvert.SerializeObject(new {command, args}));
    }

    private void OnMessageReceived(object? sender, string e)
    {
        try
        {
            var command = JsonConvert.DeserializeObject<Dictionary<string, object>>(e);
            if (command == null)
                return;
            
            var name = (string) command["command"];
            var args = (JArray) command["args"];
            
            var argsArray = new object?[args.Count];
            for (var i = 0; i < args.Count; i++)
                argsArray[i] = args[i].ToObject<object>();

            if (command.ContainsKey("rid"))
            {
                Controller.ExecuteWithReturn(name, this, (string) command["rid"], argsArray);
            }
            else
            {
                Controller.Execute(name, this, argsArray);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}