using JBPPP2.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebWindows;

namespace JBPPP2;

internal class Window
{
    private readonly WebWindow _handle;

    internal Window()
    {
        _handle = new WebWindow("JBPPP v2 (c) DasDarki");
        _handle.OnWebMessageReceived += OnMessageReceived;

#if DEBUG
        _handle.NavigateToUrl("http://localhost:5173");
#endif
    }

    internal void SendResult(string rid, object result)
    {
        SendCommand("SendResult", rid, result);
    }

    internal void WaitForExit()
    {
        _handle.WaitForExit();
    }

    internal void RegisterController<T>()
    {
        Controller.Scan<T>();
    }
    
    private void SendCommand(string command, params object[] args)
    {
        _handle.SendMessage(JsonConvert.SerializeObject(new {command, args}));
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