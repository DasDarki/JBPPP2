using Newtonsoft.Json;

namespace JBPPP2.Model;

internal class Game
{
    [JsonProperty("title")]
    internal string Title { get; set; }
    
    [JsonProperty("shortname")]
    internal string ShortName { get; set; }
    
    [JsonProperty("icon")]
    internal string Icon { get; set; }
    
    [JsonProperty("appid")]
    internal int AppId { get; set; }
    
    [JsonProperty("appname")]
    internal string AppName { get; set; }
    
    [JsonProperty("exe")]
    internal string Exe { get; set; }
    
    [JsonProperty("folder")]
    internal string Folder { get; set; }
    
    [JsonProperty("config")]
    internal string Config { get; set; }
}