using System.Collections.Concurrent;
using System.Reflection;

namespace JBPPP2.Controllers;

[AttributeUsage(AttributeTargets.Method)]
internal class Controller : Attribute
{
    private static ConcurrentDictionary<string, MethodInfo> _handlers = new();

    internal string Name { get; }
    
    internal Controller(string name) => Name = name;

    internal static void Scan<T>()
    {
        var type = typeof(T);
        
        foreach (var method in type.GetRuntimeMethods())
        {
            var attribute = method.GetCustomAttribute<Controller>();
            if (attribute is null) continue;
            
            _handlers.TryAdd(attribute.Name, method);
        }
    }
    
    internal static void Execute(string name, Window window, object?[] args)
    {
        if (!_handlers.TryGetValue(name, out var method)) return;

        var realArgs = new List<object?> {window};
        realArgs.AddRange(args);
        method.Invoke(null, realArgs.ToArray());
    }
    
    internal static void ExecuteWithReturn(string name, Window window, string rid, object?[] args)
    {
        if (!_handlers.TryGetValue(name, out var method)) return;

        var realArgs = new List<object?> {window, rid};
        realArgs.AddRange(args);
        method.Invoke(null, realArgs.ToArray());
    }
}