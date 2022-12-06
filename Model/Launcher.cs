using JBPPP2.Model.Enums;
using Newtonsoft.Json;

namespace JBPPP2.Model;

internal class Launcher
{
    [JsonProperty("path")]
    internal string Path { get; set; }
    
    [JsonProperty("type")]
    internal LauncherType Type { get; set; }
}